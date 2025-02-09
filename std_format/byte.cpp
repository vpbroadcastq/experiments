#include <cstdint>
#include <format>
#include <array>
#include <iostream>
#include <ranges>

namespace dbk {
class byte {
public:
	byte()=default;

	explicit byte(std::uint8_t b) : b_(b) {};

	std::uint8_t get() const noexcept {
		return b_;
	}

private:
	std::uint8_t b_ {};
};

} // namespace dbk

template<>
struct std::formatter<dbk::byte, char> {
	template <typename ParseContext>
	constexpr ParseContext::iterator parse(ParseContext& ctx) {
		auto it = ctx.begin();/
		while (it!=ctx.end() && *it!='}') {
			++it;
		}
		return it;
	}

	template <typename FmtContext>
	FmtContext::iterator format(dbk::byte b, FmtContext& ctx) const {
		static constexpr std::array<char,16> int2hex {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};
		std::array<char,4> bffr {'0', 'x', '0', '0'};
		bffr[2] = int2hex[b.get()>>4];
		bffr[3] = int2hex[b.get() & 0x0Fu];
		
		return std::ranges::copy(bffr, ctx.out()).out;
	}
};

int main(int argc, char* argv[]) {
	dbk::byte b1 {0x17};
	dbk::byte b2 {0xAB};
	dbk::byte b3 {0xCD};
	dbk::byte b4 {0xEF};
	std::cout << std::format("{}, {}, {}, {}", b1, b2, b3, b4) << std::endl;
	std::cout << std::format("{0}, {1}, {2}, {3}", 0x17, 0xAB, 0xCD, 0xEF) << std::endl;
	return 0;
}

