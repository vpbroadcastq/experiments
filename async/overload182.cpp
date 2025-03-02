#include <format>
#include <atomic>
#include <thread>
#include <chrono>
#include <iostream>
#include <iterator>
#include <limits>


struct non_atomic_write {
	std::uint32_t& x;
	std::uint32_t& y;
	const std::uint32_t niter;

	__attribute__((noinline)) void operator()() {
		for (std::uint32_t i=0; i<niter; ++i) { // At -O2 this loop is completely optimized away
			x = i;
			y = i;
		}
	}
};

struct non_atomic_read {
	std::uint32_t& x;
	std::uint32_t& y;
	const std::uint32_t niter;
	std::uint32_t count_mismatch {0};

	__attribute__((noinline)) void operator()() {
		for (std::uint32_t i=0; i<niter; ++i) {
			std::uint32_t yy = y;
			std::uint32_t xx = x;
			if (xx < yy) { ++count_mismatch; }
		}
	}
};


struct relaxed_relaxed_write {
	std::atomic<std::uint32_t>& x;
	std::atomic<std::uint32_t>& y;
	const std::uint32_t niter;

		__attribute__((noinline)) void operator()() {
		for (std::uint32_t i=0; i<niter; ++i) {
			x.store(i, std::memory_order_relaxed);
			y.store(i, std::memory_order_relaxed);
		}
	}
};


struct relaxed_relaxed_read {
	std::atomic<std::uint32_t>& x;
	std::atomic<std::uint32_t>& y;
	const std::uint32_t niter;
	std::uint32_t count_mismatch {0};

	__attribute__((noinline)) void operator()() {
		for (std::uint32_t i=0; i<niter; ++i) {
			std::uint32_t yy = y.load(std::memory_order_relaxed);
			std::uint32_t xx = x.load(std::memory_order_relaxed);
			if (xx < yy) { ++count_mismatch; }
		}
	}
};


struct relaxed_release_write {
	std::atomic<std::uint32_t>& x;
	std::atomic<std::uint32_t>& y;
	const std::uint32_t niter;

	__attribute__((noinline)) void operator()() {
		for (std::uint32_t i=0; i<niter; ++i) {
			x.store(i, std::memory_order_relaxed);
			y.store(i, std::memory_order_release);
		}
	}
};


struct acquire_relaxed_read {
	std::atomic<std::uint32_t>& x;
	std::atomic<std::uint32_t>& y;
	const std::uint32_t niter;
	std::uint32_t count_mismatch {0};

	__attribute__((noinline)) void operator()() {
		for (std::uint32_t i=0; i<niter; ++i) {
			std::uint32_t yy = y.load(std::memory_order_acquire);
			std::uint32_t xx = x.load(std::memory_order_relaxed);
			if (xx < yy) { ++count_mismatch; }
		}
	}
};

struct relaxed_sequential_write {
	std::atomic<std::uint32_t>& x;
	std::atomic<std::uint32_t>& y;
	const std::uint32_t niter;

	__attribute__((noinline)) void operator()() {
		for (std::uint32_t i=0; i<niter; ++i) {
			x.store(i, std::memory_order_relaxed);
			y.store(i, std::memory_order_seq_cst);
		}
	}
};


struct sequential_relaxed_read {
	std::atomic<std::uint32_t>& x;
	std::atomic<std::uint32_t>& y;
	const std::uint32_t niter;
	std::uint32_t count_mismatch {0};

	__attribute__((noinline)) void operator()() {
		for (std::uint32_t i=0; i<niter; ++i) {
			std::uint32_t yy = y.load(std::memory_order_seq_cst);
			std::uint32_t xx = x.load(std::memory_order_relaxed);
			if (xx < yy) { ++count_mismatch; }
		}
	}
};




int main(int argc, char* argv[]) {
	{	
		// Not using atomics at all
		// x and y start out ==.  t1 sets x before y, therefore expect x >= y always
		constexpr std::uint32_t niter {std::numeric_limits<std::uint32_t>::max()};
		std::uint32_t x {0};
		std::uint32_t y {0};
		non_atomic_write naw {x, y, niter};
		non_atomic_read nar {x, y, niter, 0};
		
		std::chrono::time_point<std::chrono::steady_clock> start {std::chrono::steady_clock::now()};
		std::thread t1(naw);
		std::thread t2(nar);
		t1.join();
		t2.join();
		std::chrono::time_point<std::chrono::steady_clock> end {std::chrono::steady_clock::now()};
		
		std::chrono::duration<double> time_seconds {end-start};
		std::format_to(std::ostream_iterator<char>(std::cout),"No atomics:  After {} iterations, count_mismatches == {}\n", niter, nar.count_mismatch);
		std::format_to(std::ostream_iterator<char>(std::cout),"\tElapsed time == {} seconds\n", time_seconds);
	}

	{
		// x and y start out ==.  t1 sets x before y, therefore expect x >= y always
		// All atomic operations use memory_order_relaxed
		constexpr std::uint32_t niter {std::numeric_limits<std::uint32_t>::max()};
		std::atomic<std::uint32_t> x {0};
		std::atomic<std::uint32_t> y {0};
		relaxed_relaxed_write rrw {x, y, niter};
		relaxed_relaxed_read rrr {x, y, niter, 0};
		
		std::chrono::time_point<std::chrono::steady_clock> start {std::chrono::steady_clock::now()};
		std::thread t1(rrw);
		std::thread t2(rrr);
		t1.join();
		t2.join();
		std::chrono::time_point<std::chrono::steady_clock> end {std::chrono::steady_clock::now()};
		
		std::chrono::duration<double> time_seconds {end-start};
		std::format_to(std::ostream_iterator<char>(std::cout),"memory_order_relaxed:  After {} iterations, count_mismatches == {}\n", niter, rrr.count_mismatch);
		std::format_to(std::ostream_iterator<char>(std::cout),"\tElapsed time == {} seconds\n", time_seconds);
	}

	{
		// x and y start out ==.  t1 sets x before y, therefore expect x >= y always
		// Uses memory_order_release and memory_order_acquire for one of the writes and one of the reads, respectively
		constexpr std::uint32_t niter {std::numeric_limits<std::uint32_t>::max()};
		std::atomic<std::uint32_t> x {0};
		std::atomic<std::uint32_t> y {0};
		relaxed_release_write rxrlw {x, y, niter};
		acquire_relaxed_read arr {x, y, niter, 0};
		
		std::chrono::time_point<std::chrono::steady_clock> start {std::chrono::steady_clock::now()};
		std::thread t1(rxrlw);
		std::thread t2(arr);
		t1.join();
		t2.join();
		std::chrono::time_point<std::chrono::steady_clock> end {std::chrono::steady_clock::now()};
		
		std::chrono::duration<double> time_seconds {end-start};
		std::format_to(std::ostream_iterator<char>(std::cout),"Mixed _release and _acquire:  After {} iterations, count_mismatches == {}\n", niter, arr.count_mismatch);
		std::format_to(std::ostream_iterator<char>(std::cout),"\tElapsed time == {} seconds\n", time_seconds);
	}

	{
		// x and y start out ==.  t1 sets x before y, therefore expect x >= y always
		// Uses memory_order_release and memory_order_acquire for one of the writes and one of the reads, respectively
		constexpr std::uint32_t niter {std::numeric_limits<std::uint32_t>::max()};
		std::atomic<std::uint32_t> x {0};
		std::atomic<std::uint32_t> y {0};
		relaxed_sequential_write rxsqw {x, y, niter};
		sequential_relaxed_read sqrr {x, y, niter, 0};
		
		std::chrono::time_point<std::chrono::steady_clock> start {std::chrono::steady_clock::now()};
		std::thread t1(rxsqw);
		std::thread t2(sqrr);
		t1.join();
		t2.join();
		std::chrono::time_point<std::chrono::steady_clock> end {std::chrono::steady_clock::now()};
		
		std::chrono::duration<double> time_seconds {end-start};
		std::format_to(std::ostream_iterator<char>(std::cout),"Mixed _release and _acquire:  After {} iterations, count_mismatches == {}\n", niter, sqrr.count_mismatch);
		std::format_to(std::ostream_iterator<char>(std::cout),"\tElapsed time == {} seconds\n", time_seconds);
	}



	return 0;
}


