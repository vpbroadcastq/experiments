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


int main(int argc, char* argv[]) {
	std::ostream_iterator<char> out(std::cout);
	std::format_to(out,"Process {} started; parent ID {}\n",getpid(), getppid());

	pid_t pid_child = fork();
	if (pid_child < 0) {
		std::format_to(out, "Process {}:  fork() error {}\n",getpid(), pid_child);
	} else if (pid_child == 0) {
		// Child
		std::format_to(out,"Child process {} started; parent ID {}\n",getpid(), getppid());
		pid_t pid_child_child = fork();
		if (pid_child_child < 0) {
			std::format_to(out, "Process {}:  fork() error {}\n",getpid(), pid_child_child);
		} else if (pid_child_child > 0) {
			// The first child
			std::format_to(out, "Process {}:  Forked child with ID {}\n",getpid(), pid_child_child);
			std::format_to(out, "Process {}:  calling exit(0)\n",getpid());
			exit(0);
		}

		// The second child
		std::format_to(out,"Child process {} started; parent ID {}\n",getpid(), getppid());
		std::format_to(out,"Process {} sleeping for 2 seconds\n",getpid());
		sleep(2);
		// At this point, the parent of this child is gone (and so is the parent of /that/ process)
		std::format_to(out, "Process {}:  just woke up; parent={}; calling exit(0)\n",getpid(), getppid());
		exit(0);
	}

	// Parent
	std::format_to(out, "Process {}:  Forked child with ID {}\n",getpid(), pid_child);
	std::format_to(out,"Process {} calling waitpid() on child process {}\n",getpid(), pid_child);
	pid_t pid_wait = waitpid(pid_child, nullptr, 0);
	if (pid_wait != pid_child) {
		std::format_to(out, "Process {}:  waitpid() error {}\n",getpid(), pid_wait);
	}

	std::format_to(out,"Process {} returning 0 from main\n",getpid());
	return 0;
}







