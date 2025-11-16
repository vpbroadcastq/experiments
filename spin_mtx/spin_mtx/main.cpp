#include "spin_mtx.h"
#include <iostream>
#include <thread>
#include <format>
#include <iterator>
#include <chrono>


void test() {
	{
		spin_mtx m;
		verify(m.try_lock());
	}

	{
		spin_mtx m;
		m.lock();
		verify(!m.try_lock());
	}
}

// Increment val n times; i is protected by the mutex m.  Returns the number of iterations required
// to do the full increment.
// Locking strategy is to call .lock(); expect n_iter == n
int increment_n_lock(spin_mtx& m, int& val, int n) {
	int n_iter {0};
	int i {0};
	while (i<n) {
		m.lock();
		++i;
		++n_iter;
		++val;
		m.unlock();
	}
	return n_iter;
}

// Increment val n times; i is protected by the mutex m.  Returns the number of iterations required
// to do the full increment.
// Locking strategy is to call .try_lock(); expect n_iter > n
int increment_n_trylock(spin_mtx& m, int& val, int n) {
	int n_iter {0};
	int i {0};
	while (i<n) {
		if (!m.try_lock()) {
			++n_iter;
			continue;
		}
		++val;
		++i;
		++n_iter;
		m.unlock();
	}
	return n_iter;
}

// Same as the two above but does not protect val with the mtx.  It's obviously illegal
// to access val from multiple threads without synchronization.
int increment_n_nolock(spin_mtx& m, int& val, int n) {
	int n_iter {0};
	int i {0};
	while (i<n) {
		++i;
		++n_iter;
		++val;
	}
	return n_iter;
}



// This demo program is intended to show the behavior of a spinlock mutex and how proper
// synchronization prevents race conditions in multi-threaded code.
// The program runs two main experiments with 2 threads each incrementing a shared
// variable 100,000 times:
//
// Experiment 1: With Synchronization (Correct)
//  Thread 1 uses increment_n_lock(): Calls lock() before every increment
//  Thread 2 uses increment_n_trylock(): Uses try_lock(), retrying on failure
//  Expected result: Final value = 200,000 (correct sum of all increments)
//  Shows that proper mutex protection ensures thread-safe access
//
// Experiment 2: Without Synchronization (Data Race)
//  Both threads use increment_n_nolock(): No mutex protection
//  Expected result: Final value < 200,000 (race condition causes lost updates)
//  Demonstrates why synchronization is necessary when multiple threads access shared
//  data.
// The program measures the number of iterations each approach takes, showing the
// performance overhead of try_lock() (which fails repeatedly) versus lock()
// (which waits), while illustrating that the correctness of the result depends on
// proper synchronization.
int main(int argc, char* argv[]) {
	std::ostream_iterator<char> out(std::cout);

	test();

	// Using the spin_mtx to protect a variable being incremented
	// Expect the final value of the variable to be 2*100'000 (2 is the number of threads)
	{
		std::format_to(out, "Example 1:  Correct synchronization\n");
		const int N {100'000};
		int val {0};
		int n_iter_lock {0};
		int n_iter_trylock {0};
		spin_mtx m;
		std::chrono::time_point start = std::chrono::steady_clock::now();
		std::thread t1([&n_iter_lock,&val,N,&m](){n_iter_lock = increment_n_lock(m,val,N);});
		std::thread t2([&n_iter_trylock,&val,N,&m](){n_iter_trylock = increment_n_trylock(m,val,N);});
		t1.join();
		t2.join();
		std::chrono::time_point end = std::chrono::steady_clock::now();
		std::chrono::duration<double> elapsed = end - start;
		std::format_to(out, "  For N={} increment steps\n",N);
		std::format_to(out, "  increment_n_lock took {} iterations; increment_n_trylock took {} iterations\n",n_iter_lock,n_iter_trylock);
		std::format_to(out, "  val final == {}\n",val);
		std::format_to(out, "  Elapsed time: {} microseconds\n",
			std::chrono::duration_cast<std::chrono::microseconds>(elapsed).count());
	}

	// Not using any sort of mutex to protect a variable being incremented.  This is not expected to work, ie, the
	// final value of the variable will not be 2*100'000, because you can't access the same value from ultiple
	// threads without synchronization.
	{
		std::format_to(out, "Example 2:  No synchronization (incorrect)\n");
		const int N {100'000};
		int val {0};
		int n_iter_lock {0};
		int n_iter_trylock {0};
		spin_mtx m;
		std::chrono::time_point start = std::chrono::steady_clock::now();
		std::thread t1([&n_iter_lock,&val,N,&m](){n_iter_lock = increment_n_nolock(m,val,N);});
		std::thread t2([&n_iter_trylock,&val,N,&m](){n_iter_trylock = increment_n_nolock(m,val,N);});
		t1.join();
		t2.join();
		std::chrono::time_point end = std::chrono::steady_clock::now();
		std::chrono::duration<double> elapsed = end - start;
		std::format_to(out, "  For N={} increment steps\n",N);
		std::format_to(out, "  increment_n_nolock took {} iterations; increment_n_nolock took {} iterations\n",n_iter_lock,n_iter_trylock);
		std::format_to(out, "  val final == {}\n",val);
		std::format_to(out, "  Elapsed time: {} microseconds\n",
			std::chrono::duration_cast<std::chrono::microseconds>(elapsed).count());
	}

	return 0;
}
