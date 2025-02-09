#include <format>
#include <ranges>
#include <iostream>
#include <cstdint>
#include <vector>
#include <span>
#include <charconv>
#include <cstring>

namespace dbk {
class ieee754 {
public:
	constexpr ieee754()=default;
	constexpr ieee754(double d) noexcept : d_(d) {}

	double get() const noexcept {
		return d_;
	}
private:
	double d_ {};
};

} // namespace dbk

template<>
struct std::formatter<dbk::ieee754> {
	// Has to return the iterator pointing to the terminating '}', not ctx.end()!
	template<typename ParseCtx>
	constexpr ParseCtx::iterator parse(ParseCtx& ctx) {
		auto it = ctx.begin();
		while (it!=ctx.end() && *it!='}') {
			++it;
		}
		return it;
	}

	template<typename FmtCtx>
	FmtCtx::iterator format(const dbk::ieee754& v, FmtCtx& ctx) const {
		// sign followed by exponent followed by significand
		// [seee'eeee][eeee'mmmm][mmmm'mmmm][mmmm'mmmm][mmmm'mmmm][mmmm'mmmm][mmmm'mmmm][mmmm'mmmm]
		// But, LE, so:
		// [mmmm'mmmm][mmmm'mmmm][mmmm'mmmm][mmmm'mmmm][mmmm'mmmm][mmmm'mmmm][eeee'mmmm][seee'eeee]
		double d = v.get();
		const std::uint8_t* p = reinterpret_cast<const std::uint8_t*>(&d);
		
		bool is_neg = ((*(p+7))&0x80u) != 0;
		
		std::int16_t exp {};
		exp = (*(p+7))&0x7Fu;
		exp <<= 4;
		exp += ((*(p+6))>>4);

		std::uint64_t s {};
		static_assert(sizeof(double)==sizeof(std::uint64_t));
		std::memcpy(&s, p, sizeof(std::uint64_t));
		s = s&(0x00'00'FF'FF'FF'FF'FF'FFu);  // [eeee'mmmm][seee'eeee] are the high bits
		s >>= 8;
		s += (*(p+6))&0x0Fu;
		//std::reverse(reinterpret_cast<std::uint8_t*>(&s), reinterpret_cast<std::uint8_t*>(&s)+sizeof(std::uint64_t));

		// TODO:  Too big
		std::array<char,256> bffr {};
		char* const dest_end = bffr.data() + bffr.size();
		char* dest =  bffr.data();
		if (is_neg) {
			*dest++ = '-';
		} else {
			*dest++ = '+';
		}
		std::to_chars_result res_sig = std::to_chars(dest, dest_end, s);
		dest = res_sig.ptr;
		*dest++ = ' ';
		*dest++ = '^';
		*dest++ = ' ';

		std::to_chars_result res_exp = std::to_chars(dest, dest_end, exp);
		dest = res_exp.ptr;

		auto it = ctx.out();
		
		return std::ranges::copy(bffr.data(), dest, it).out;
	}
};







int main(int argc, char* argv[]) {
	std::vector<double> vals {1.0, -1.0, 2.0, -2.0, 0.5, -0.5, 3.0, -3.0, 4.0, -4.0};
	for (double curr : vals) {
		std::cout << std::format("{}:  {}\n", curr, dbk::ieee754(curr));
		
		// As array of bytes
		std::span<std::uint8_t> s {reinterpret_cast<unsigned char*>(&curr), sizeof(double)};
		for (const std::uint8_t b : s) {
			std::cout << std::format("{:02x}, ", b);
		}
		std::cout << '\n';

		// As array of bits
		std::uint64_t u64 {};
		std::memcpy(&u64, &curr, sizeof(double));
		const uint8_t* pu8 = reinterpret_cast<const std::uint8_t*>(&u64);
		for (int bytenum=0; bytenum<sizeof(std::uint64_t); ++bytenum) {
			std::cout << '[';
			for (int bitnum=0; bitnum<8; ++bitnum) {
				std::uint8_t mask {0b1000'0000u}; // NB:  Printing the _high_ bits first b/c it prints ltr!
				mask >>= bitnum;
				int bitval = (((*pu8)&mask) > 0) ? 1 : 0;
				std::cout << std::format("{}", bitval);
				if (bitnum==3) {
					std::cout << '\'';
				}
			}
			std::cout << ']';
			++pu8;
		}
		
		std::cout << "\n\n";
	}

	return argc;
}
