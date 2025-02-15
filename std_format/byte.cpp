#include <cstdint>
#include <format>
#include <array>
#include <iostream>
#include <ranges>
#include <vector>
#include <iterator>
#include <span>
#include <bit>


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


class i1 {
public:
	virtual std::uint64_t get() const = 0;
};

class i2 {
public:
	virtual std::uint64_t get2() const = 0;
};

class i3 {
public:
	virtual std::uint64_t get3() const = 0;
};



class a : public i1 {
public:
	a()=default;
	explicit a(std::uint64_t val) : m_data(val) {}

	std::uint64_t get() const override {
		return m_data;
	}
private:
	std::uint64_t m_data {0xAA'AA'AA'AA'AA'AA'AA'AAu};
};

class b : public a {
public:
	b()=default;
	explicit b(std::uint64_t val) : m_data(val) {}

	std::uint64_t get() const override {
		return m_data;
	}
private:
	std::uint64_t m_data {0xBB'BB'BB'BB'BB'BB'BB'BBu};
};

class c : public i1, public i2 {
public:
	c()=default;
	explicit c(std::uint64_t val) : m_data(val) {}

	std::uint64_t get() const override {
		//const std::uintptr_t pthis = std::bit_cast<const std::uintptr_t,decltype(this)>(this);
		const std::uintptr_t pthis = reinterpret_cast<const std::uintptr_t>(this);
		std::cout << std::format("c::get() (i1) => this == {:'d}\n", dbk::as_bytes(pthis));
		return m_data;
	}

	std::uint64_t get2() const override {
		//const std::uintptr_t pthis = std::bit_cast<const std::uintptr_t,decltype(this)>(this);
		const std::uintptr_t pthis = reinterpret_cast<const std::uintptr_t>(this);
		std::cout << std::format("c::get2() (i2) => this == {:'d}\n", dbk::as_bytes(pthis));
		return m_data + 2;
	}
private:
	std::uint64_t m_data {0xCC'CC'CC'CC'CC'CC'CC'CCu};
};

class d : public c, public i3 {
public:
	d()=default;
	explicit d(std::uint64_t val) : m_data(val) {}

	std::uint64_t get() const override {
		const std::uintptr_t pthis = reinterpret_cast<const std::uintptr_t>(this);
		std::cout << std::format("d::get() (i1) => this == {:'d}\n", dbk::as_bytes(pthis));
		return m_data;
	}

	std::uint64_t get2() const override {
		const std::uintptr_t pthis = reinterpret_cast<const std::uintptr_t>(this);
		std::cout << std::format("d::get2() (i2) => this == {:'d}\n", dbk::as_bytes(pthis));
		return m_data + 2;
	}

	std::uint64_t get3() const override {
		const std::uintptr_t pthis = reinterpret_cast<const std::uintptr_t>(this);
		std::cout << std::format("d::get3() (i3) => this == {:'d}\n", dbk::as_bytes(pthis));
		return m_data + 3;
	}

private:
	std::uint64_t m_data {0xDD'DD'DD'DD'DD'DD'DD'DDu};
};



int main(int argc, char* argv[]) {
	{
		std::cout << "Example 1:  A bunch of uint8_t's\n";
		std::uint8_t b1 {0x17};
		std::uint8_t b2 {0xAB};
		std::uint8_t b3 {0xCD};
		std::uint8_t b4 {0xEF};
		std::cout << std::format("{}, {}, {}, {}", dbk::as_bytes(b1), dbk::as_bytes(b2), dbk::as_bytes(b3), dbk::as_bytes(b4)) << std::endl;
		std::cout << std::format("{0}, {1}, {2}, {3}", b1,b2,b3,b4) << "\n\n";
	}

	{
		std::cout << "Example 2:  A uint64_t\n";
		std::uint64_t qw1 {0x12'34'56'78'9A'BC'DE'F0u};
		std::cout << std::format("{:'w}", dbk::as_bytes(qw1)) << std::endl;
		std::cout << std::format("{}", qw1) << "\n\n";
	}

	{
		std::cout << "Example 3:  An array (size==2) of uint64_t\n";
		std::array<std::int64_t,2> arr1 {-15, 0x00'BB'CC'DD'EE'FF'11'22};
		std::cout << std::format("arr1 {{-15, 0x00'BB'CC'DD'EE'FF'11'22}} == {:'q}", dbk::as_bytes(arr1)) << "\n\n";
	}
	
	{
		std::cout << "Example 4:  A vector<int>\n";
		std::vector<int> v1 {1,2,3};
		std::cout << std::format("std::vector<int> v1 {{1,2,3}} == {:'q}", dbk::as_bytes(v1)) << "\n\n";
	}

	{
		std::cout << "Example 5:  A small string\n";
		std::string s1 {"potato"};
		std::cout << std::format("std::string s1 {{\"potato\"}} == {:'q}", dbk::as_bytes(s1)) << "\n\n";
	}

	std::cout << "\n----- OBJECT LAYOUT -----\n";
	{
		std::cout << "Example 1:  a : public i1\n";
		a a1 {};
		std::cout << std::format("a a1() == {:|q}", dbk::as_bytes(a1)) << "\n";
		a a2 {0xFF'FF'FF'FF'FF'FF'FF'F1u};
		std::cout << std::format("a a2(int max-15) == {:|q}", dbk::as_bytes(a2)) << "\n\n";
		
		std::cout << "Example 3:  b : public a\n";
		b b1 {};
		std::cout << std::format("b b1() == {:|q}", dbk::as_bytes(b1)) << "\n\n";

		std::cout << "Example 4:  c : public i1, public i2\n";
		c c1 {};
		std::cout << std::format("c c1() == {:|q}", dbk::as_bytes(c1)) << "\n\n";
	
		std::cout << "Example 5:  d : public c, public i3\n";
		d d1 {};
		std::cout << std::format("d d1() == {:|q}", dbk::as_bytes(d1)) << "\n\n";
	}

	std::cout << "\n----- POINTERS AND CASTING -----\n";
	{
		std::cout << "Example 1:  a : public i1\n";
		a a1 {};
		a* pa1 = &a1;
		i1* pa1_i1 = static_cast<i1*>(&a1);
		std::cout << std::format("a a1() == {:|q}", dbk::as_bytes(a1)) << "\n";
		std::cout << std::format("&a1 == {:'d}; static_cast<i1*>(&a1) == {:'d}\n",
			dbk::as_bytes(pa1), dbk::as_bytes(pa1_i1));
		std::cout << "\n";
	}
	{
		std::cout << "Example 2:  b : public a\n";
		b b1 {};
		b* pb1 = &b1;
		a* pb1_a = static_cast<a*>(&b1);
		i1* pb1_i1 = static_cast<i1*>(&b1);
		std::cout << std::format("b b1() == {:|q}", dbk::as_bytes(b1)) << "\n\n";
		std::cout << std::format("&b1 == {:'d}; static_cast<a*>(&b1) == {:'d}; static_cast<i1*>(&b1) == {:'d}\n",
			dbk::as_bytes(pb1), dbk::as_bytes(pb1_a), dbk::as_bytes(pb1_i1));
		std::cout << "\n";
	}
	{
		std::cout << "Example 3:  c : public i1, public i2\n";
		c c1 {};
		c* pc1 = &c1;
		i1* pc1_i1 = static_cast<i1*>(&c1);
		i2* pc1_i2 = static_cast<i2*>(&c1);
		std::cout << std::format("c c1() == {:|q}", dbk::as_bytes(c1)) << "\n\n";
		std::cout << std::format("&c1 == {:'d}\nstatic_cast<i1*>(&c1) == {:'d}\nstatic_cast<i2*>(&c1) == {:'d}\n",
			dbk::as_bytes(pc1), dbk::as_bytes(pc1_i1), dbk::as_bytes(pc1_i2));
		std::cout << "Note how the i2* points at the second vtbl ptr!\n";
		std::cout << "c1.get() => "; c1.get();
		std::cout << "c1.get2() => "; c1.get2();
		std::cout << "pc1_i1.get() => "; pc1_i1->get();
		std::cout << "pc1_i2.get2() => "; pc1_i2->get2();
		std::cout << "\n";
	}
	{
		std::cout << "Example 4:  d : public c, public i3\n";
		d d1 {};
		d* pd1 = &d1;
		i1* pd1_i1 = static_cast<i1*>(&d1);
		i2* pd1_i2 = static_cast<i2*>(&d1);
		i3* pd1_i3 = static_cast<i3*>(&d1);
		c* pd1_c = static_cast<c*>(&d1);
		std::cout << std::format("d d1() == {:|q}", dbk::as_bytes(d1)) << "\n\n";
		std::cout << std::format("&d1 == {:'d}\nstatic_cast<i1*>(&d1) == {:'d}\nstatic_cast<i2*>(&d1) == {:'d}\nstatic_cast<i3*>(&d1) == {:'d}\nstatic_cast<c*>(&d1) == {:'d}\n",
			dbk::as_bytes(pd1), dbk::as_bytes(pd1_i1), dbk::as_bytes(pd1_i2), dbk::as_bytes(pd1_i3), dbk::as_bytes(pd1_c));
		std::cout << "Note how the i2* points at the second vtbl ptr and how the i3* points at the third!\n";
		std::cout << "d1.get() => "; d1.get();
		std::cout << "d1.get2() => "; d1.get2();
		std::cout << "d1.get3() => "; d1.get3();
		std::cout << "pd1_i1.get() => "; pd1_i1->get();
		std::cout << "pd1_i2.get2() => "; pd1_i2->get2();
		std::cout << "pd1_i3.get3() => "; pd1_i3->get3();
		std::cout << "pd1_c.get() => "; pd1_c->get();
		std::cout << "pd1_c.get2() => "; pd1_c->get2();
		std::cout << "\n";
	}

	return 0;
}


