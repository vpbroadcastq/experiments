#include <iostream>
#include <string>
#include <format>
#include <thread>
#include <cstdint>
#include <atomic>
#include <iterator>
#include <ostream>

std::atomic<int> g_num_calls_a {0};
int a() {
	++g_num_calls_a;
	return g_num_calls_a;
}

int get_int(int i) {
	return i;
}

// If get_magic_static is called from multiple threads, 'magic' will only be initialized
// once, but how many times will a() be called?
int get_magic_static() {
	static int magic = get_int(a());
	return magic;
}

int main(int argc, char* argv[]) {
	int t1_result {0};
	int t2_result {0};

	std::thread t1([&t1_result](){t1_result = get_magic_static();});
	std::thread t2([&t2_result](){t2_result = get_magic_static();});
	t1.join();
	t2.join();

	std::format_to(std::ostream_iterator<char>(std::cout),"g_num_calls_a == {}\n", g_num_calls_a.load());
	std::format_to(std::ostream_iterator<char>(std::cout),"get_magic_static() == {}\n", get_magic_static());
	std::format_to(std::ostream_iterator<char>(std::cout),"t1_result == {}\n", t1_result);
	std::format_to(std::ostream_iterator<char>(std::cout),"t2_result == {}\n", t2_result);

	return 0;
}





