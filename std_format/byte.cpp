#include <cstdint>
#include <format>
#include <array>
#include <iostream>
#include <ranges>
#include <vector>
#include <iterator>


namespace dbk {

class as_bytes {
public:
	as_bytes()=delete;
	as_bytes(const as_bytes&)=delete;
	as_bytes(as_bytes&&)=delete;
	as_bytes& operator=(const as_bytes&)=delete;
	as_bytes& operator=(as_bytes&&)=delete;

	template <typename T>
	explicit as_bytes(const T& t) : p_(&t), sz_(sizeof(T)) { }

	template<typename T>
	explicit as_bytes(T& t) : p_(&t), sz_(sizeof(T)) { }

	template<typename T>
	explicit as_bytes(T&&)=delete;

	std::size_t get_size() const noexcept {
		return sz_;
	}

	std::span<const std::uint8_t> get() const noexcept {
		return {reinterpret_cast<const std::uint8_t*>(p_), sz_};
	}

private:
	const void* p_;
	std::size_t sz_;
};

} // namespace dbk

template<>
struct std::formatter<dbk::as_bytes, char> {
	std::optional<char> m_bytesep {};
	std::uint8_t m_unit {1};  // The number of bytes per seperator
	
	template <typename ParseContext>
	constexpr ParseContext::iterator parse(ParseContext& ctx) {
		auto it = ctx.begin();
		while (it!=ctx.end() && *it!='}') {
			if (*it=='b') { // byte
				m_unit = 1;
			} else if (*it=='w') { // word
				m_unit = 2;
			} else if (*it=='d') { // dword
				m_unit = 4;
			} else if (*it=='q') { // quadword
				m_unit = 8;
			} else {
				m_bytesep = *it;
			}
			++it;
		}
		return it;
	}

	template <typename FmtContext>
	FmtContext::iterator format(const dbk::as_bytes& b, FmtContext& ctx) const {
		static constexpr std::array<char,16> int2hex {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};
		std::span<const std::uint8_t> s = b.get();
		auto it = ctx.out();
		std::uint32_t i {0};
		for (const std::uint8_t b : s) {
			if (m_bytesep && (i>0 && (i)%m_unit==0)) {
				*it++ = *m_bytesep;
			}
			++i;
			*it++ = int2hex[b>>4];
			*it++ = int2hex[b & 0x0Fu];
		}
		return it;
	}
};

int main(int argc, char* argv[]) {
	std::uint8_t b1 {0x17};
	std::uint8_t b2 {0xAB};
	std::uint8_t b3 {0xCD};
	std::uint8_t b4 {0xEF};
	std::cout << std::format("{}, {}, {}, {}", dbk::as_bytes(b1), dbk::as_bytes(b2), dbk::as_bytes(b3), dbk::as_bytes(b4)) << std::endl;
	std::cout << std::format("{0}, {1}, {2}, {3}", b1,b2,b3,b4) << std::endl;
	
	std::uint64_t qw1 {0x12'34'56'78'9A'BC'DE'F0u};
	std::cout << std::format("{:'w}", dbk::as_bytes(qw1)) << std::endl;
	std::cout << std::format("{}", qw1) << std::endl;
	
	std::array<std::int64_t,2> arr1 {-15, 0x00'BB'CC'DD'EE'FF'11'22};
	std::cout << std::format("{:'q}", dbk::as_bytes(arr1)) << std::endl;

	std::vector<int> v1 {1,2,3};
	std::cout << std::format("{:'q}", dbk::as_bytes(v1)) << std::endl;

	std::string s1 {"potato"};
	std::cout << std::format("{:'q}", dbk::as_bytes(s1)) << std::endl;

	return 0;
}


