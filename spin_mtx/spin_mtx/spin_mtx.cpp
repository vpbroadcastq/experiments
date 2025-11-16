#include "spin_mtx.h"
#include <exception>

void verify(bool b) {
	if (!b) {
		std::terminate();
	}
}

void spin_mtx::lock() {
	while (true) {
		bool expected = false;
		bool result = m_locked.compare_exchange_weak(expected,true, std::memory_order_acquire);

		/*if (expected) {
			verify(!result);
		}
		if (!expected) {
			verify(result);
		}*/

		if (!expected) {
			break;
		}
	}
}

void spin_mtx::unlock() {
	m_locked = false;
}

// True if the lock was successfully taken, ie, the caller now owns the locked state and is responsible
// for calling unlock().  False if somebody else owned the lock at the time of the call (the caller does
// _not_ own the locked state).
bool spin_mtx::try_lock() {
	bool expected = false;
	bool result = m_locked.compare_exchange_strong(expected,true, std::memory_order_release);
	return result;
}

