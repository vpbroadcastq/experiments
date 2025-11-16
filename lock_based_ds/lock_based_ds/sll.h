#include <memory>
#include <cstdint>

template<typename T>
class sll {
	// !val_ => !next_
	struct node {
		std::unique_ptr<node> next_;
		std::shared_ptr<T> val_;
	};

	node head_;

public:
	sll()=default;

	void push_front(std::shared_ptr<T> val) {
		std::unique_ptr<node> old_head = std::make_unique<node>();
		old_head->next_ = std::move(head_.next_);
		old_head->val_ = std::move(head_.val_);

		head_.next_ = std::move(old_head);
		head_.val_ = std::move(val);
	}

	// f(const T&)
	template<typename Fn>
	void for_each(Fn f) {
		const node* p = &head_;
		while (p && p->val_) {
			f(*(p->val_.get()));
			p = p->next_.get();
		}
	}

	// pred(const T&)
	template<typename Pr>
	std::shared_ptr<T> find_first_if(Pr pred) {
		const node* p = &head_;
		while (p && p->val_) {
			if (pred(*(p->val_.get()))) {
				return p->val_;
			}
			p = p->next_.get();
		}
		return nullptr;
	}

	template<typename Pr>
	std::shared_ptr<T> remove_if(Pr pred) {
		node* p = &head_;
		node* prev {nullptr};
		while (p && p->val_) {
			if (pred(*(p->val_.get()))) {
				break;
			}
			prev = p;
			p = p->next_.get();
			continue;
		}

		if (!p) {
			return nullptr;
		}

		// Remove
		if (p == &head_) {
			// Removing the very first node in the list
			std::shared_ptr<T> result = std::move(p->val_);
			head_ = std::move(*(p->next_));
			return result;
		} else if (!p->val_) {
			// Removing the last node in the list
			std::shared_ptr<T> result = std::move(p->val_);
			prev->next_ = nullptr;
			return result;
		} else {
			// Removing an interior node
			std::shared_ptr<T> result = std::move(p->val_);
			prev->next_ = std::move(p->next_);
			return result;
		}
	}

};


