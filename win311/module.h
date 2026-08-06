#ifndef include_module_h
#define include_module_h

#include <string.h>
#include <windows.h> 
#include <toolhelp.h>
#include "util.h"

struct module {
	HMODULE h; 
	WORD refcount;
	char name[MAX_MODULE_NAME+1];
	//char exe_path[MAX_PATH+1];
};      

module to_module(const MODULEENTRY& me);                     

class module_list {
	module* beg_;
	module* last_;
	module* end_;
	static_assert(sizeof(module*)==2);
public:
	module_list();
	~module_list();
	
	module& push_back(const module& m);
	int size() const;
	int capacity() const;
	void reserve(int newcap);
	module& operator[](int n);
	const module& operator[](int n) const;
};                                              

const module* find(HANDLE h, const module_list& ml);  

#endif
  