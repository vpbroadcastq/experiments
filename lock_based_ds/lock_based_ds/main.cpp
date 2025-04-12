#include "lock_based_stack.h"
#include "lock_based_queue.h"
#include "sll_q.h"
#include <iostream>

int main(int argc, char* argv[]) {
	std::cout << "Hello CMake." << std::endl;

	sll_queue<int> q1;
	q1.push(1);
	q1.push(2);
	q1.push(3);

	auto val = q1.try_pop();
	val = q1.try_pop();
	val = q1.try_pop();

	return 0;
}
