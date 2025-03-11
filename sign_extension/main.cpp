#include <iostream>
#include <string>
#include <cstdint>
#include <format>
#include <array>
#include <ostream>
#include <iterator>


extern "C" {
uint64_t movsx(const char*);
uint64_t movzx(const char*);
}

int main(int argc, char* argv[]) {
	std::array<char,5> a {-2, -1, 0, 1, 2};
	for (const char& ch : a) {
		uint64_t result_sx = movsx(&ch);
		uint64_t result_zx = movzx(&ch);
		std::format_to(std::ostream_iterator<char>(std::cout),"ch={}; movsx(ch)=={}; movzx(ch)=={}\n",
				static_cast<int>(ch),result_sx,result_zx);
		if (ch < 0) {
			std::format_to(std::ostream_iterator<char>(std::cout),"\tInterpreted as a signed int64, movsx(ch)=={}; movzx(ch)=={}\n",
				static_cast<int64_t>(result_sx),static_cast<int64_t>(result_zx));
		}
	}


	return 0;
}





