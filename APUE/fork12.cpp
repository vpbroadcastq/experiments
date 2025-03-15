#include <iostream>
#include <format>
#include <iterator>
#include <vector>

#include <stdio.h>
#include <stdlib.h>
#include <sys/wait.h>
#include <unistd.h>

int main() {
	std::ostream_iterator<char> oit(std::cout);
	std::format_to(oit,"%");;

	std::vector<char> input(1024,'\0');
	while (std::fgets(input.data(), input.size(), stdin)) {
		std::size_t input_len = strlen(input.data());
		if (input[input_len-1] == '\n') {
			input[input_len-1] = '\0';
		}
		
		pid_t pid = fork();
		if (pid < 0) {
			std::format_to(oit,"Fork error :(");
		} else if (pid == 0) {
			// Child
			execlp(input.data(), input.data(), nullptr);
			std::format_to(oit, "couldn't execute: {}", input.data());
			std::exit(127);
		}

		// Parent
		int status {};
		pid = waitpid(pid, &status, 0);
		if (pid < 0) {
			std::format_to(oit, "waitpid error");
		}
		std::format_to(oit, "%");
	}

	return 0;
}


