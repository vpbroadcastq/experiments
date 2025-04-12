#pragma once
#include <vector>
#include <mutex>

// More or less Williams p.176

template<typename T>
class ts_stack {
private:
	std::vector<T> m_data;
	std::mutex m_mtx;

public:
	ts_stack()=default;

	ts_stack(const ts_stack& rhs) {
		std::lock_guard g_rhs(rhs.m_mtx);
		m_data = rhs.m_data;
	}

	ts_stack& operator=(const ts_stack& rhs) {
		std::lock_guard g_rhs(rhs.m_mtx);
		std::lock_guard g_lhs(m_mtx);
		m_data = rhs.m_data;
		return *this;
	}

	void push(const T& val) {
		std::lock_guard g(m_mtx);
		m_data.push_back(val);
		// What if T's copy ctor throws?  It's ok as long as the move ctor is noexcept.  The only problematic
		// scenario is the one where T can't be copy-inserted _and_ the move ctor can throw.
	}

	void push(T&& val) {
		std::lock_guard g(m_mtx);
		m_data.push_back(std::move(val));
	}


	bool pop(T& dest) {
		std::lock_guard g(m_mtx);
		if (m_data.empty()) {
			return false;
		}
		dest = std::move(m_data.back());
		m_data.pop_back();
		return true;
	}

	std::shared_ptr<T> pop() {
		std::lock_guard g(m_mtx);
		if (m_data.empty()) {
			return {};
		}
		std::shared_ptr<T> result = std::move(m_data.back());
		m_data.pop_back();
		return result;
	}

	bool empty() const {
		std::lock_guard g(m_mtx);
		return m_data.empty();
	}
};

