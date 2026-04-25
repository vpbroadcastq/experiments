#pragma once

#include <vector>
#include <mutex>
#include <optional>

// No way to wait for items.  For a useful MT stack you typically want pop() to block
// until an item is available (using std::condition_variable) rather than returning
// std::nullopt and forcing the caller to spin.

template <typename T>
class mt_stack {
    std::vector<T> data_;
    std::mutex mtx_;

public:
    // SMF
    mt_stack() = default;
    mt_stack(size_t cap) {
        data_.reserve(cap);
    }
    mt_stack(const mt_stack&) = delete;
    mt_stack& operator=(const mt_stack&) = delete;
    mt_stack(mt_stack&&) = delete;
    mt_stack& operator=(mt_stack&&) = delete;
    ~mt_stack() = default;

    // ...
    std::optional<T> pop() {
        std::scoped_lock lk(mtx_);
        std::optional<T> o {};
        if (data_.empty()) {
            return o;
        }
        o = std::move(data_.back());
        data_.pop_back();
        return o;
    }

    void push(T val) {
        std::scoped_lock lk(mtx_);
        data_.push_back(std::move(val));
    }
};

