#include <array>
#include <algorithm>
#include <span>
#include <ranges>


//
// Compile-time sorting
// Verify with strings a.out | grep aabc
//

template<typename T>
consteval T sort_wrapper(T&& r) {
	std::ranges::sort(r);
	return r;
}


std::span<const char> get() {
	static constexpr std::array<char,10> a = sort_wrapper(std::array<char,10>{'c','d','a','e','c','b','f','e','a','g'});
	return a;
}

int main(int argc, char* argv[]) {
	if (argc >= 0 && argc < get().size()) {
		return get()[argc];
	}
	return 1;
}




