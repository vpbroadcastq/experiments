#pragma once
#include <atomic>

class spin_mtx {
private:
	std::atomic<bool> m_locked {false};
public:
	// Blocks via a spin lock
	void lock();

	void unlock();

	// True if the lock was successfully taken, ie, the caller now owns the locked state and is responsible
	// for calling unlock().  False if somebody else owned the lock at the time of the call (the caller does
	// _not_ own the locked state).
	bool try_lock();
};

void verify(bool b);
