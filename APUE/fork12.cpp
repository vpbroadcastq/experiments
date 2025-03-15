#include <iostream>
#include <format>
#include <iterator>
#include <string>

#include <sys/wait.h>
#include <unistd.h>

int main() {
	std::ostream_iterator<char> oit(std::cout);

	std::string input;
	input.reserve(1024);
	while (true) {
		std::format_to(oit,"%");;
		std::getline(std::cin, input);
		if (!std::cin) {
			break;
		}
		
		pid_t pid = fork();
		if (pid < 0) {
			std::format_to(oit,"Fork error :(");
		} else if (pid == 0) {
			// Child
			execlp(input.c_str(), input.c_str(), nullptr);
			std::format_to(oit, "couldn't execute: {}\n", input.c_str());
			std::exit(127);
		}

		// Parent
		int status {};
		pid = waitpid(pid, &status, 0);
		if (pid < 0) {
			std::format_to(oit, "waitpid error");
		}
	}

	return 0;
}


