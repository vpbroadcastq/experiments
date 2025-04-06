#include "spin_mtx.h"
#include <iostream>
#include <thread>
#include <format>
#include <iterator>
#include <mutex>


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

// Same as the two above but does not protect val with the mtx
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


int main(int argc, char* argv[]) {
	std::ostream_iterator<char> out(std::cout);

	test();

	// Using the spin_mtx to protect a variable being incremented
	{
		const int N {100'000};
		int val {0};
		int n_iter_lock {0};
		int n_iter_trylock {0};
		spin_mtx m;
		std::thread t1([&n_iter_lock,&val,N,&m](){n_iter_lock = increment_n_lock(m,val,N);});
		std::thread t2([&n_iter_trylock,&val,N,&m](){n_iter_trylock = increment_n_trylock(m,val,N);});
		t1.join();
		t2.join();
		std::format_to(out, "For N={} increment steps\n",N);
		std::format_to(out, "increment_n_lock took {} iterations; increment_n_trylock took {} iterations\n",n_iter_lock,n_iter_trylock);
		std::format_to(out, "val final == {}\n",val);
	}

	// Not using any sort of mutex to protect a variable being incremented
	{
		const int N {100'000};
		int val {0};
		int n_iter_lock {0};
		int n_iter_trylock {0};
		spin_mtx m;
		std::thread t1([&n_iter_lock,&val,N,&m](){n_iter_lock = increment_n_nolock(m,val,N);});
		std::thread t2([&n_iter_trylock,&val,N,&m](){n_iter_trylock = increment_n_nolock(m,val,N);});
		t1.join();
		t2.join();
		std::format_to(out, "For N={} increment steps\n",N);
		std::format_to(out, "increment_n_nolock took {} iterations; increment_n_nolock took {} iterations\n",n_iter_lock,n_iter_trylock);
		std::format_to(out, "val final == {}\n",val);
	}

	return 0;
}
