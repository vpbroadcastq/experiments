#include <iostream>
#include <vector>
#include <thread>
#include <string>
#include <format>
#include <cstdint>
#include <random>
#include <chrono>
#include <mutex>
#include <memory>

enum class thread_name {
	main,
	async,
	none
};

class expensive {
public:
	expensive() {
		std::random_device rd;
		std::default_random_engine re(rd());
		std::uniform_int_distribution rdist(0,2);
		m_sleep_init = std::chrono::milliseconds{rdist(re)};
		std::this_thread::sleep_for(m_sleep_init);
	}

private:
	std::chrono::milliseconds m_sleep_init {};
};

class thing : public std::enable_shared_from_this<thing> {
public:
	thing() {}

	void init(std::chrono::microseconds sleep) {
		std::weak_ptr<thing> wpthis = weak_from_this();
		auto async_init = [wpthis,sleep](){
			std::this_thread::sleep_for(sleep*2);
			std::shared_ptr<thing> spthis = wpthis.lock();
			if (!spthis) {
				return;
			}
			if (!spthis->m_mtx.try_lock()) {
				return;
			}
			if (spthis->m_expensive) {
				spthis->m_mtx.unlock();
				return;
			}
			spthis->m_expensive = std::make_unique<expensive>();
			spthis->m_who_init = thread_name::async;
			spthis->m_mtx.unlock();
		};
		//std::this_thread::sleep_for(sleep);
		std::thread t(async_init);
		t.detach();
	}

	expensive& get() {
		m_mtx.lock();
		if (!m_expensive) {
			this->m_who_init = thread_name::main;
			m_expensive = std::make_unique<expensive>();
		}
		m_mtx.unlock();
		return *m_expensive;
	}

	thread_name who() const {
		return m_who_init;
	}

private:
	std::unique_ptr<expensive> m_expensive;
	thread_name m_who_init {thread_name::none};
	std::mutex m_mtx;
};


int main(int argc, char* argv[]) {
	std::vector<thread_name> init_record;

	std::random_device rd;
	std::default_random_engine re(rd());
	std::uniform_int_distribution rdist(0,10);

	for (int i=0; i<10000; ++i) {
		std::shared_ptr<thing> the_thing = std::make_shared<thing>();
		the_thing->init(std::chrono::microseconds {rdist(re)});
		std::chrono::microseconds main_sleep {rdist(re)};
		std::this_thread::sleep_for(main_sleep);

		expensive& e = the_thing->get();
		// Def. initialized at this point
		init_record.push_back(the_thing->who());
	}

	std::size_t n_main {0};
	auto it = init_record.cbegin();
	while (it != init_record.cend()) {
		int i=0;
		while (i<50 && it != init_record.cend()) {
			if (*it == thread_name::main) { ++n_main; }
			std::cout << static_cast<int>(*it) << " ";
			++it;
			++i;
		}
		std::cout << '\n';
	}
	std::cout << 100*(static_cast<double>(n_main)/static_cast<double>(init_record.size())) << "% main thread\n";
	std::cout << std::flush;

	return 0;
}
