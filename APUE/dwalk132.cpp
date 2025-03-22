#include <format>
#include <iterator>
#include <iostream>
#include <string>
#include <cstddef>
#include <functional>
#include <algorithm>
#include <ranges>

#include <dirent.h>
#include <sys/stat.h>


std::string_view filetype_to_string(const struct stat& s) {
	switch(s.st_mode & S_IFMT) {
		case S_IFREG: return "Regular";
		case S_IFBLK: return "Block";
		case S_IFCHR: return "Character";
		case S_IFIFO: return "FIFO";
		case S_IFLNK: return "Link";
		case S_IFSOCK: return "Socket";
		case S_IFDIR: return "Directory";
		default: return "Unknown";
	}
}


// TODO:  What does the return value signify?
// f(const std::string&, stat*) -> bool
//bool walk(std::string& path, std::ostream_iterator<char> it, std::function<bool(const std::string&, const struct stat*)>& f) {
template <typename F>
bool walk(std::string& path, std::ostream_iterator<char> it, F& f) {
	//std::format_to(it,"walk({})\n",path);
	struct stat statres {};
	if (lstat(path.c_str(), &statres) < 0) {
		// stat error
		f(path, nullptr);
		return false;
	}

	if (S_ISDIR(statres.st_mode) == 0) {
		// Not a directory; visit it & return
		return f(path, &statres);
	}

	// Path is a directory; visit it
	
	if (!f(path, &statres)) {
		return false;
	}

	//
	// Open the directory and iterate over the contents
	//

	std::size_t init_len = path.size();
	path += '/';
	
	DIR* dp = opendir(path.c_str());
	if (!dp) {
		// Can't read directory
		// Here, the example calls f again, but i already called it on path above...
		return false;
	}

	while (true) {
		struct dirent* dirp = readdir(dp);
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


struct visitor {
	std::ostream_iterator<char> m_oit;

	int n_reg {0};
	int n_blk {0};
	int n_chr {0};
	int n_fifo {0};
	int n_lnk {0};
	int n_sock {0};
	int n_dir {0};
	int n_unk {0};

	bool operator()(const std::string& s, const struct stat* statres) {
		int n = std::ranges::count(s, '/');
		if (n >= 1) {
			n -= 1;
		}
		for (int i=0; i<n; ++i) {
			*m_oit = '\t';
			++m_oit;
		}
		//std::format_to(m_oit, "{}\t{}\n",s,filetype_to_string(*statres));
		
		switch((statres->st_mode) & S_IFMT) {
			case S_IFREG: ++n_reg; break;
			case S_IFBLK: ++n_blk; break; 
			case S_IFCHR: ++n_chr; break;
			case S_IFIFO: ++n_fifo; break;
			case S_IFLNK: ++n_lnk; break;
			case S_IFSOCK: ++n_sock; break;
			case S_IFDIR: ++n_dir; break;
			default: ++n_unk;
		}		
		
		return true;
	}
};


int main(int argc, char* argv[]) {
	std::ostream_iterator<char> out(std::cout);
	if (argc != 2) {
		std::format_to(out, "Usage: dirwalk <path>\n");
		return 1;
	}

	// TODO:  Trim any trailing / from argv[1]
	std::string path = argv[1];
	visitor v {out};
	walk(path,out,v);

	std::format_to(out, "\nSTATS:\n");
	std::format_to(out, "Regular files:  {}\n", v.n_reg);
	std::format_to(out, "Block special:  {}\n", v.n_blk);
	std::format_to(out, "Character special:  {}\n", v.n_chr);
	std::format_to(out, "FIFOs:  {}\n", v.n_fifo);
	std::format_to(out, "Symbolic links:  {}\n", v.n_lnk);
	std::format_to(out, "Sockets:  {}\n", v.n_sock);
	std::format_to(out, "Directories:  {}\n", v.n_dir);
	std::format_to(out, "Unknown:  {}\n", v.n_unk);

	return 0;
}


