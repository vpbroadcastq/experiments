#pragma once
#include <queue>
#include <mutex>
#include <condition_variable>
#include <memory>

//
// Following the example in Ch 4 of C++ Concurrency in Action (Anthony Williams)
//

template<typename T>
class ts_queue {
private:
	std::mutex _mtx;
	std::queue<T> _q;
	std::condition_variable _cv;
public:
	ts_queue() {
		//...
	}

	ts_queue(const ts_queue& lhs) {
		std::lock_guard g(lhs._mtx);
		_q = lhs._q;
	}

	void push(T v) {
		std::lock_guard g(_mtx);
		_q.push(v);
	}

	// "wait" means "wait for it to be !empty"
	void wait_and_pop(T& dest) {  // use cv???
		//...
	}

	// "wait" means "wait for it to be !empty"
	std::shared_ptr<T> wait_and_pop() {
		//...
	}

	bool try_pop(&T v) {
		//...
	}

	std::shared_ptr<T> try_pop() {
		//...
	}

	bool empty() const {
		std::lock_guard g(_mtx);
		return _q.empty();
	}
};
