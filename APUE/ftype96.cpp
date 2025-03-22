#include <format>
#include <iostream>
#include <iterator>

#include <sys/stat.h>

int main(int argc, char* argv[]) {
	std::ostream_iterator<char> out(std::cout);

	for (int i=1; i<argc; ++i) {
		std::format_to(out,"{}:\n",argv[i]);

		struct stat lstat_result {};
		int lstat_ret = lstat(argv[i], &lstat_result);
		if (lstat_ret < 0) {
			std::format_to(out, "lstat error {}\n", lstat_ret);
			continue;
		}

		//
		// Type from .st_mode
		//
		std::format_to(out, "\tType from st_mode:  ");
		if (S_ISREG(lstat_result.st_mode)) {
			std::format_to(out, "regular\n");
		} else if (S_ISDIR(lstat_result.st_mode)) {
			std::format_to(out, "directory\n");
		} else if (S_ISCHR(lstat_result.st_mode)) {
			std::format_to(out, "character special\n");
		} else if (S_ISBLK(lstat_result.st_mode)) {
			std::format_to(out, "block special\n");
		} else if (S_ISFIFO(lstat_result.st_mode)) {
			std::format_to(out, "fifo\n");
		} else if (S_ISLNK(lstat_result.st_mode)) {
			std::format_to(out, "symbolic link\n");
		} else if (S_ISSOCK(lstat_result.st_mode)) {
			std::format_to(out, "socket\n");
		} else {
			std::format_to(out, "*** unknown ***\n");
		}

		//
		// .st_ino
		//
		std::format_to(out,"\ti-node number (st_ino):  {}\n",lstat_result.st_ino);

		//
		// .st_nlink
		//
		std::format_to(out, "\tNumber of links (st_nlink):  {}\n", lstat_result.st_nlink);

		//
		// user and group id .st_uid, .st_gid
		//
		std::format_to(out, "\tUser ID:  {}\n\tGroup ID:  {}\n", lstat_result.st_uid, lstat_result.st_gid);
	}

	return 0;
}

