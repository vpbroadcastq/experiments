#include <type_traits>
#include <concepts>
#include <memory>

class b {
public:
	virtual int get() const = 0;
};

class d1 : public b {
public:
	virtual int get() const override {
		return 1;
	}
	bool set(int i) {
		m_val = i;
		return true;
	}
private:
	int m_val {};
};

class d2 : public b {
	virtual int get() const override {
		return 1;
	}
};

class e {
public:
	int get() const {
		return 2;
	}
	bool set(int i) {
		m_val = i;
		return true;
	}
private:
	int m_val {};
};

template<typename T>
concept MyConcept = std::is_base_of<b,T>::value && requires(T* t) { t->set(1); };

template<MyConcept T>
int f(const T* t) {
	return t->get()+2;
}

int main(int argc, char* argv[]) {
	std::unique_ptr<d1> pd1 = std::make_unique<d1>();
	std::unique_ptr<d2> pd2 = std::make_unique<d2>();
	std::unique_ptr<e> pe = std::make_unique<e>();

	// d1 has a set function that accepts an int and is derrived from b, so it should be possible
	// to call f.
	int i_d1 = f(pd1.get());

	// d2 derives from b but does not have a set function that accepts an int.  This should not
	// compile.
	//int i_d2 = f(pd2.get());

	// e has a set function that accepts an int, but does not derive from b.  This should not compile.
	//int i_e = f(pe.get());

	return 0;
}
