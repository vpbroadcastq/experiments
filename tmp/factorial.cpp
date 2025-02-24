#include <iostream>
#include <cstdint>


template<std::uint32_t n>
struct factorial {
	enum {
		value = n*factorial<n-1>::value
	};
};

template<>
struct factorial<0> {
	enum {
		value = 1
	};
};


int main(/*int argc , char* argv[]*/) {

	std::cout << "factorial<5>::value == " << factorial<5>::value << "\n";

	return factorial<5>::value;
}
























