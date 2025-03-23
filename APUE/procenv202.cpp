#include <iostream>
#include <iterator>
#include <format>
#include <string>
#include <cstdint>
#include <array>
#include <string_view>
#include <cstdlib>
#include <unistd.h>

extern char** environ;  // Where is this declared?

constexpr std::array<std::string_view,20> envvars {
	"COLUMNS",
	"DATEMASK",
	"HOME",
	"LANG",
	"LC_ALL",
	"LC_COLLATE",
	"LC_CTYPE",
	"LC_MESSAGES",
	"LC_MONETARY",
	"LC_NUMERIC",
	"LC_TIME",
	"LINES",
	"LOGNAME",
	"MSGVERB",
	"NLSPATH",
	"PATH",
	"PWD",
	"SHELL",
	"TERM",
	"TEMPDIR",
};


int main(int argc, char* argv[]) {
    std::ostream_iterator<char> out(std::cout);

	std::format_to(out,"***** Command line *****\n");
	for (int i=0; i<argc; ++i) {
		std::format_to(out,"argv[{}]={}\n",i,argv[i]);
	}
	*out++ = '\n';

	std::format_to(out,"***** Envvars via char** environ *****\n");
	char** p = environ;
	int i {0};
    while (p && *p) {
        std::format_to(out,"environ[{}]={},\n",i,*p);
    	++p;
    	++i;
	}
	*out++ = '\n';

	std::format_to(out,"***** Selected envvars via getenv() *****\n");
	for (const std::string_view s : envvars) {
		char* c = getenv(s.data());
		if (!c) {
			continue;
		}
		std::format_to(out,"{}={}\n",s,c);
	}
	*out++ = '\n';

	std::format_to(out,"***** Process identifiers *****\n");
	std::format_to(out,"Process ID getpid()={}\n",getpid());
	std::format_to(out,"Process ID of parent getppid()={}\n",getppid());
	std::format_to(out,"Real user ID getuid()={}\n",getuid());
	std::format_to(out,"Effective user ID geteuid()={}\n",geteuid());
	std::format_to(out,"Real group ID getgid()={}\n",getgid());
	std::format_to(out,"Effective group ID getegid()={}\n",getegid());

	return 0;
}







