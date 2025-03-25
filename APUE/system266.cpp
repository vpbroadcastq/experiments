#include <iostream>
#include <iterator>
#include <format>
#include <string>
#include <cstdint>
#include <array>
#include <string_view>
#include <cstdlib>
#include <unistd.h>
#include <sys/wait.h>

int system(std::string_view cmd) {
	int child_status {};  // termination status of child
	pid_t pid_child = fork();
	if (pid_child < 0) {
		return -1;
	} else if (pid_child == 0) {
		// Child
		execl("/bin/sh", "sh", "-c", cmd.data(), nullptr);
		_exit(127);  // exec error
	} else {
		// Parent
		int wait_result = waitpid(pid_child, &child_status,0);
		// wait_result is the pid of the child that terminated... always == pid_child unless error???
		if (wait_result < 0 && errno != EINTR) {
			return -1;
		}
	}
	return child_status;
}


int main(int argc, char* argv[]) {
	std::ostream_iterator<char> out(std::cout);
	std::format_to(out,"Running system():\n");
	for (int i=1; i<argc; ++i) {
		std::format_to(out,"\tsystem({}) = ",argv[i]);
		int result = system(argv[i]);
		std::format_to(out,"{}\n", result);
	}

	return 0;
}







