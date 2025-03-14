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
		std::unique_lock lk(_mtx);
		_cv.wait(lk,[this](){ return !_q.empty(); });
		dest = _q.front();
		_q.pop();
		return;
	}

	// "wait" means "wait for it to be !empty"
	std::shared_ptr<T> wait_and_pop() {
		std::unique_lock lk(_mtx);
		_cv.wait(lk,[this](){ return !_q.empty(); });
		auto result = std::make_shared(_q.front());
		_q.pop();
		return result;
	}

	bool try_pop(T& v) {
		std::lock_guard g(_mtx);
		if (_q.empty()) {
			return false;
		}
		v = _q.front();
		_q.pop();
	}

	std::shared_ptr<T> try_pop() {
		std::lock_guard g(_mtx);
		if (_q.empty()) {
			return nullptr;
		}
		auto result = std::make_shared(_q.front());
		_q.pop();
	}

	bool empty() const {
		std::lock_guard g(_mtx);
		return _q.empty();
	}
};
