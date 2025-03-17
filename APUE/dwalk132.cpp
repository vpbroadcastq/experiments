#include <format>
#include <iterator>
#include <iostream>
#include <string>
#include <cstddef>
#include <functional>

#include <dirent.h>
#include <sys/stat.h>


// TODO:  What does the return value signify?
// f(const std::string&, stat*) -> bool
bool walk(std::string& path, std::ostream_iterator<char> it, std::function<bool(const std::string&, struct stat*, std::ostream_iterator<char>)> f) {
	std::format_to(it,"walk({})\n",path);
	struct stat statres {};
	if (lstat(path.c_str(), &statres) < 0) {
		// stat error
		f(path, nullptr, it);
		return false;
	}

	if (S_ISDIR(statres.st_mode) == 0) {
		// Not a directory
		return f(path, &statres, it);
	}

	// path is a directory
	if (!f(path, &statres, it)) {
		return false;
	}
	std::size_t init_len = path.size();
	path += '/';
	
	DIR* dp = opendir(path.c_str());
	if (!dp) {
		// Can't read directory
		// Here, the example calls f again, but i already called it on path above...
		return false;
	}

	struct dirent* dirp {};
	while (true) {
		dirp = readdir(dp);
		if (!dirp) {
			break;
		}
		if (strcmp(dirp->d_name, ".")==0 || strcmp(dirp->d_name, "..")==0) {
			continue;
		}
		path.resize(init_len+1);
		path += dirp->d_name;
		if (!walk(path,it,f)) { // recursive call
			break;
		}
	}
	path.resize(init_len);  // why?
	
	if (closedir(dp) < 0) {
		return false;  // Not quite right... will short circuit the process
	}

	return true;
}


bool myfunc(const std::string& s, struct stat* statres, std::ostream_iterator<char> it) {
	std::format_to(it, "\tmyfunc({}, stat*)\n",s);
	return true;
}


int main(int argc, char* argv[]) {
	std::ostream_iterator<char> out(std::cout);
	if (argc != 2) {
		std::format_to(out, "Usage: dirwalk <path>\n");
		return 1;
	}

	// TODO:  Trim any trailing /
	std::string path = argv[1];
	walk(path,out,myfunc);
	



	return 0;
}


