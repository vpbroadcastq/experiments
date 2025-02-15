#include <iostream>
#include <string>
#include <format>


class c {
private:
	std::string m_str {"potato"};
};



//
// Templates
//
template<typename T>
void f(const T&) {
	std::cout << "called void f(const T&\n";
	return;
}

template<typename T>
void f(T&&) {
	std::cout << "called void f(T&&)\n";
	return;
}



//
// Nontemplates
//
void g(const c&) {
	std::cout << "called void g(const c&)\n";
	return;
}

void g(c&&) {
	std::cout << "called void f(c&&)\n";
	return;
}


int main(int argc, char* argv[]) {
	c my_c1;
	std::cout << "c my_c1;\n";
	std::cout << "f(my_c1); => "; f(my_c1);
	std::cout << "f(std::move(my_c1)); => "; f(std::move(my_c1));
	std::cout << "\n\n";

	c my_c2;
	std::cout << "c my_c2;\n";
	std::cout << "g(my_c2); => "; g(my_c2);
	std::cout << "g(std::move(my_c2)); => "; g(std::move(my_c2));
	std::cout << "\n\n";


	return 0;
}








