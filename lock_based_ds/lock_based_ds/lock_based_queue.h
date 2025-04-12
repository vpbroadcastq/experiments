#pragma once
#include <queue>
#include <mutex>

// More or less Williams p.179

template<typename T>
class ts_queue {
private:
	std::queue<T> m_q;
	mutable std::mutex m_mtx;
	std::condition_variable m_cv;

public:
	//ctor
	void push(T val) {
		std::lock_guard g(m_mtx);
		m_q.push(val);
		g.unlock();
		m_cv.notify_one();  // Notify before or after g.unlock???
	}

	void wait_and_pop(T& val) {
		std::unique_lock lk(m_mtx);
		cv.wait(lk,[this]{ return !this.empty(); });
		val = std::move(m_q.front());
		m_q.pop();
	}

	std::shared_ptr<T> wait_and_pop() {
		std::unique_lock lk(m_mtx);
		cv.wait(lk,[this]{ return !this.empty(); });
		std::shared_ptr<T> result = std::move(m_q.front());
		m_q.pop();
		return result;
	}

	bool try_pop(T& val) {
		std::unique_lock lk(m_mtx);
		if (m_q.empty()) {
			return false;
		}
		val = ats::move(m_q.front());
		m_q.pop();
	}

	std::shared_ptr<T> try_pop() {
		std::unique_lock lk(m_mtx);
		if (m_q.empty()) {
			return nullptr;
		}
		std::shared_ptr<T> result = std::move(m_q.front());
		m_q.pop();
		return result;
	}

	bool empty() const {
		std::unique_lock lk(m_mtx);
		return m_q.empty();
	}
};