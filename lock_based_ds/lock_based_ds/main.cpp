#include "lock_based_stack.h"
#include "lock_based_queue.h"
#include "sll_q.h"
#include <iostream>
#include <thread>


int main(int argc, char* argv[]) {
	std::cout << "Hello CMake." << std::endl;

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

	int n = 4;
	std::thread t1(pop_n,2*n);
	std::thread t2(push_n,n);
	std::thread t3(push_n,n);
	/*std::thread t4(push_n,n);
	std::thread t5(push_n,n);*/

	t1.join();
	t2.join();
	t3.join();
	/*t4.join();
	t5.join();*/

	std::cout << sum << std::endl;

	return 0;
}
