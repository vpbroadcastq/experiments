#include <type_traits>

static_assert(std::is_signed_v<char>);

int main(int argc, char* argv[]) {
	return argc;
}


