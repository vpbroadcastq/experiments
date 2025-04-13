#include <memory>
#include <mutex>

// Singly-linked-list-based queue
// Williams p. 183-184

// Single-threaded version
template<typename T>
class sll_queue {
	struct node {
		T val_ {};
		std::unique_ptr<node> next_ {};
	};
	std::unique_ptr<node> head_ {};
	node* tail_ {};

public:
	sll_queue()=default;
	sll_queue(const sll_queue&)=delete;
	sll_queue& operator=(const sll_queue&)=delete;

	void push(T val) {
		if (tail_) {  // q !empty
			tail_->next_ = std::make_unique<node>(std::move(val),nullptr);
			tail_ = tail_->next_.get();
		} else {  // q empty
			head_ = std::make_unique<node>(val,nullptr);
			tail_ = head_.get();
		}
	}

	std::unique_ptr<T> try_pop() {
		if (head_) {
			std::unique_ptr<T> result = std::make_unique<T>(std::move(head_->val_));
			if (tail_==head_.get()) {
				tail_ = nullptr;
			}
			head_ = std::move(head_->next_);
			return result;
		}
		return nullptr;
	}
};



// Multi-threaded version
// You'd think it would be possible to push and pop at the same time given something like this
template<typename T>
class tssll_queue {
	struct node {
		T val_ {};
		std::unique_ptr<node> next_ {};
	};
	std::unique_ptr<node> head_ {};
	node* tail_ {};

	std::mutex mtx_head_;
	std::mutex mtx_tail_;

public:
	tssll_queue()=default;
	tssll_queue(const tssll_queue&)=delete;
	tssll_queue& operator=(const tssll_queue&)=delete;

	void push(T val) {
		std::lock_guard lkt(mtx_tail_);
		if (tail_) {  // q !empty
			tail_->next_ = std::make_unique<node>(std::move(val),nullptr);
			tail_ = tail_->next_.get();
		} else {  // q empty
			std::lock_guard lkh(mtx_head_);
			head_ = std::make_unique<node>(val,nullptr);
			tail_ = head_.get();
		}
	}

	std::unique_ptr<T> try_pop() {
		std::lock_guard lkh(mtx_head_);
		if (head_) {
			std::unique_ptr<T> result = std::make_unique<T>(std::move(head_->val_));
			mtx_tail_.lock();
			if (tail_==head_.get()) {
				tail_ = nullptr;
			}
			mtx_tail_.unlock();
			head_ = std::move(head_->next_);
			return result;
		}
		return nullptr;
	}
};



