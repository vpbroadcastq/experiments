#include "compare_exchange.h"
#include <format>
#include <atomic>
#include <iostream>
#include <iterator>


int main(int argc, char* argv[]) {
	std::ostream_iterator<char> out(std::cout);
	std::atomic<bool> b {false};
	
	// In the two cases below, compare_exchange_weak is used to set b to true.  In case 1 b starts out false,
	// in case 2 it starts out true.  Our value for 'expected' is alwys false.  

	// b is initially false
	{
		const bool b_init = false;
		b = b_init;
		bool expected_init {false};
		bool expected {expected_init};
		bool cex_result {};
		int niter {0};
		while (true) {
			cex_result = b.compare_exchange_weak(expected,true);
			if (cex_result || expected) {
				break;
			}
			++niter;
		}
		std::format_to(out, "b_init={}, expected_init={}, niter={}, b_final={}, expected_final={} cex_result={}\n",
			b_init, expected_init, niter, b.load(), expected, cex_result);
	}

	// b is initially true
	{
		const bool b_init = true;
		b = b_init;
		bool expected_init {false};
		bool expected {expected_init};
		bool cex_result {};
		int niter {0};
		while (true) {
			cex_result = b.compare_exchange_weak(expected,true);
			if (cex_result || expected) {
				break;
			}
			++niter;
		}
		std::format_to(out, "b_init={}, expected_init={}, niter={}, b_final={}, expected_final={} cex_result={}\n",
			b_init, expected_init, niter, b.load(), expected, cex_result);
	}

	!(a && b) => !a || !b

	return 0;
}
