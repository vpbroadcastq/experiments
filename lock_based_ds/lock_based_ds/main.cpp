#include "lock_based_stack.h"
#include "lock_based_queue.h"
#include "sll_q.h"
#include "map.h"
#include <iostream>
#include <thread>
#include <random>
#include <string>

bool test_tssl_queue() {
	tssll_queue<int> q1;

	auto push_n = [&q1](int n) {
		for (int i=0; i<n; ++i) {
			q1.push(i);
		}
	};

	std::int64_t sum {0};

	auto pop_n = [&q1,&sum](int n) {
		int npop {0};
		while (npop < n) {
			auto curr = q1.try_pop();
			if (curr) {
				++npop;
				sum += *curr;
			}
		}
	};

	int n = 1000;
	std::thread t1(pop_n,4*n);
	std::thread t2(push_n,n);
	std::thread t3(push_n,n);
	std::thread t4(push_n,n);
	std::thread t5(push_n,n);

	t1.join();
	t2.join();
	t3.join();
	t4.join();
	t5.join();

	std::cout << sum << std::endl;

	return true;
}

bool test_map() {
	map<int,std::string> m(19);
	constexpr int max_key = 1000;

	std::random_device rd;
    std::mt19937 gen(rd());
    std::uniform_int_distribution<> distrib(1,2);
	for (int i=0; i<10000; ++i) {
		int rand_op = distrib(gen);
		if (rand_op==1) {  // Add/update
			int k = i%(max_key+1);
			std::string val = std::to_string(k);
			m.add_or_update(k,val);
			if (!m.get(k) || m.get(k) != val) { std::abort(); }
		} else if (rand_op==2) {  // Remove
			int k = i%(max_key+1);
			std::string val = std::to_string(k);
			bool is_initially_present = (m.get(k)!=std::nullopt);
			bool removed = m.remove(k);
			if ((is_initially_present && !removed) || (!is_initially_present && removed)) {
				std::abort();
			}
		}
	}

	//m.add_or_update(0,"zero");
	//m.add_or_update(1,"one");
	//m.add_or_update(2,"two");

	return true;
}


void random_add_remove_update(tsmap<int,std::string>& m) {
	constexpr int max_key = 1000;

	std::random_device rd;
    std::mt19937 gen(rd());
    std::uniform_int_distribution<> distrib(1,2);
	for (int i=0; i<10000; ++i) {
		int rand_op = distrib(gen);
		if (rand_op==1) {  // Add/update
			int k = i%(max_key+1);
			std::string val = std::to_string(k);
			m.add_or_update(k,val);
		} else if (rand_op==2) {  // Remove
			int k = i%(max_key+1);
			bool removed = m.remove(k);
		}
	}
}


bool test_tsmap() {
	tsmap<int,std::string> m(19);

	auto threadproc = [&m]() {
		random_add_remove_update(m);
	};

	std::thread t1(threadproc);
	std::thread t2(threadproc);
	std::thread t3(threadproc);
	std::thread t4(threadproc);
	std::thread t5(threadproc);

	t1.join();
	t2.join();
	t3.join();
	t4.join();
	t5.join();

	return true;
}


int main(int argc, char* argv[]) {
	std::cout << "Hello CMake." << std::endl;

	//test_tssl_queue();
	//test_map();
	test_tsmap();

	return 0;
}
