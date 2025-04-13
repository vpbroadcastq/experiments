#include <mutex>
#include <vector>
#include <list>
#include <utility>
#include <memory>
#include <optional>
#include <ranges>

// Non thread-safe version
template<typename K, typename V, typename H = std::hash<K>>
class map {
	class bucket_t {
	public:
		std::list<std::pair<K,V>> data_;
	};

	std::vector<std::unique_ptr<bucket_t>>  buckets_;

	std::size_t idx(const K& k) const {
		H h{};
		return h(k)%(buckets_.size());
	}

	struct find_result_t {
		std::pair<K,V>* entry;
		std::list<std::pair<K,V>>::iterator it;
		bucket_t* bucket;  // Never null
	};
	find_result_t find(const K& k) const {
		const std::unique_ptr<bucket_t>& b = buckets_[idx(k)];
		auto it = std::ranges::find_if(b.get()->data_,
			[&k](const std::pair<K,V>& p){ return p.first==k; });
		if (it == b.get()->data_.end()) {
			return {nullptr, it, buckets_[idx(k)].get()};
		}
		return {&(*it), it, buckets_[idx(k)].get()};
	}

public:
	map(std::size_t num_buckets) {  // pick a prime number
		for (size_t i=0; i<num_buckets; ++i) {
			buckets_.push_back(std::make_unique<bucket_t>());
		}
	}

	std::optional<V> get(const K& k) {
		find_result_t fr = find(k);
		if (!fr.entry) {
			return std::nullopt;
		}
		return fr.entry->second;
	}

	bool add_or_update(const K& k, V v) {
		find_result_t fr = find(k);
		if (!fr.entry) {
			fr.bucket->data_.push_back({k,std::move(v)});
			return true;
		}
		fr.entry->second = std::move(v);
		return false;
	}


	bool remove(const K& k) {
		find_result_t fr = find(k);
		if (!fr.entry) {
			return false;
		}
		fr.bucket->data_.erase(fr.it);
		return true;
	}
};




