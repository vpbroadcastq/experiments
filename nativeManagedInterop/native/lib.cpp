#include <cstdint>
#include <print>
#include <string>

//
// GlobalPrinter
//
class GlobalPrinter {
    public:
    GlobalPrinter() {
        m_id = id;
        std::print("C++:  GlobalPrinter constructed; id=={}\n", m_id);
        ++id;
    }
    ~GlobalPrinter() {
        std::print("C++:  GlobalPrinter shutting down; id=={}\n", m_id);
    }

    private:
    int m_id{};
    static int id;
};

GlobalPrinter gp{};
constinit int GlobalPrinter::id = 0;


//
// NonTrivial
//
class NonTrivial {
    public:
    NonTrivial() {
        std::print("C++:  NonTrivial::NonTrivial()\n");
        m_str = "This class owns a std::string";
    }
    ~NonTrivial() {
        std::print("C++:  NonTrivial::~NonTrivial()\n");
    }
    
    const char* GetPtr();

    private:
    std::string m_str;
};
static_assert(sizeof(NonTrivial)==32);

const char* NonTrivial::GetPtr() {
    std::print("C++:  Calling NonTrivial::GetPtr()\n");
    return m_str.c_str();
}


//
// Assorted free functions
//
extern "C" {

NonTrivial MakeNonTrivial() {
    std::print("C++:  Calling MakeNonTrivial()\n");
    NonTrivial nt{};
    return nt;
}

int32_t add0() {
    return 0;
}

int32_t add1(int32_t a) {
    return a;
}

int32_t add2(int32_t a, int32_t b) {
    return a + b;
}

int32_t add3(int32_t a, int32_t b, int32_t c) {
    return a + b + c;
}

int32_t add4(int32_t a, int32_t b, int32_t c, int32_t d) {
    return a + b + c + d;
}

int32_t add5(int32_t a, int32_t b, int32_t c, int32_t d, int32_t e) {
    return a + b + c + d + e;
}

int32_t add6(int32_t a, int32_t b, int32_t c, int32_t d, int32_t e, int32_t f) {
    return a + b + c + d + e + f;
}

int32_t add7(int32_t a, int32_t b, int32_t c, int32_t d, int32_t e, int32_t f, int32_t g) {
    return a + b + c + d + e + f + g;
}


} // extern "C"






