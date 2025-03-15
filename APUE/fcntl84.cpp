#include <string>
#include <format>
#include <iostream>
#include <iterator>
#include <cstdlib>
#include <charconv>
#include <string.h>

#include <fcntl.h>

int main(int argc, char* argv[]) {
	std::ostream_iterator<char> out(std::cout);
	if (argc != 2) {
		std::format_to(out, "Usage: a.out <descriptor#>\n");
		return 1;
	}

	int fd {};
	{
		size_t len = strlen(argv[1]);
		std::from_chars_result r = std::from_chars(argv[1], argv[1]+len, fd, 10);
		if (r.ec != std::errc()) {
			std::format_to(out, "Error parsing file descriptor\n");
			return 2;
		}
	}

	int flags = fcntl(fd, F_GETFL, 0);
	if (flags < 0) {
		std::format_to(out, "Error:  fcntl returned {} for fd {}\n", flags, fd);
		return 3;
	}

	std::string result;
	const int accmode = flags & O_ACCMODE;
	if (accmode == O_RDONLY) {
		result += "Read only";
	} else if (accmode == O_WRONLY) {
		result += "Write only";
	} else if (accmode == O_RDWR) {
		result += "Read write";
	} else {
		result += "Unknown access mode";
	}

	if (flags & O_APPEND) {
		result += ", append";
	}
	if (flags & O_NONBLOCK) {
		result += ", nonblocking";
	}
	if (flags & O_SYNC) {
		result += ", synchronous writes";
	}

	std::format_to(out,"{}\n", result);

	return 0;
}




