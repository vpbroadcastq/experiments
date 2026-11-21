#include <cstdint>
#include <print>
#include <string>
#include <cstring>
#include <algorithm>

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
// String
//
class HorribleString {
    char* m_p {nullptr};  // always \0-terminated
    ptrdiff_t m_sz {0};  // does not include the \n

    public:
    //
    // SMF's
    //
    HorribleString();
    explicit HorribleString(char* src) noexcept;
    ~HorribleString();
    HorribleString(const HorribleString& rhs);
    HorribleString(HorribleString&& rhs);
    //
    // Operators
    //
    HorribleString& operator=(const HorribleString& rhs);
    HorribleString& operator=(HorribleString&& rhs);

    //
    // Other 
    //
    const char* Get() const;
    char* Get();
    ptrdiff_t Size() const;
    void Set(char* src);
};
static_assert(sizeof(HorribleString)==16);


HorribleString::HorribleString()=default;

HorribleString::HorribleString(char* src) noexcept {
    if (!src) {
        return;
    }

    size_t n = std::strlen(src);
    m_p = new char[n+1];  // +1 because strlen does not include the \0
    char* dest = std::copy(src, src+n, m_p);
    *dest = '\0';
    m_sz = static_cast<ptrdiff_t>(n); // recorded size does not include the \0
}

HorribleString::~HorribleString() {
    delete[] m_p;
}

HorribleString::HorribleString(const HorribleString& rhs) {
    m_p = new char[rhs.Size()+1];
    m_sz = static_cast<ptrdiff_t>(rhs.Size());
    char* dest = std::copy(rhs.Get(), rhs.Get()+rhs.Size(), m_p);
    *dest = '\0';
}

HorribleString::HorribleString(HorribleString&& rhs) {
    m_p = rhs.Get();
    m_sz = rhs.Size();
    rhs.m_p = nullptr;
    rhs.m_sz = 0;
}

//
// Operators
//
HorribleString& HorribleString::operator=(const HorribleString& rhs) {
    if (this == &rhs) {
        return *this;
    }
    if (rhs.Size() == 0) {
        delete[] m_p;
        m_p = nullptr;
        m_sz = 0;
        return *this;
    }
    if (rhs.Size() > m_sz) {
        delete[] m_p;
        m_p = new char[rhs.Size()+1];
    }
    char* dest = std::copy(rhs.Get(), rhs.Get()+rhs.Size(), m_p);
    *dest = '\0';
    m_sz = rhs.Size();
    return *this;
}

HorribleString& HorribleString::operator=(HorribleString&& rhs) {
    delete[] m_p;
    m_p = rhs.Get();
    m_sz = rhs.Size();
    rhs.m_p = nullptr;
    rhs.m_sz = 0;
    return *this;
}

//
// Other 
//
const char* HorribleString::Get() const {
    return m_p;
}

char* HorribleString::Get() {
    return m_p;
}

ptrdiff_t HorribleString::Size() const {
    return m_sz;
}

void HorribleString::Set(char* src) {
   if (!src) {
        delete[] m_p;
        m_p = nullptr;
        m_sz = 0;
        return;
    }

    // Bug:  This prevents substring assignment
    if (m_p && (src >= m_p && src <= m_p + m_sz)) {
        return;
    }
        
    ptrdiff_t n = strlen(src);
    if (n > m_sz) {
        delete[] m_p;
        m_p = new char[n+1];
    }
    if (!m_p) {
        m_p = new char[n+1];
    }
    char* dest = std::copy(src, src+n, m_p);
    *dest = '\0';  // valid because m_sz does not account for the space allocated for the \0
    m_sz = n;
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






