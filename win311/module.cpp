#include "module.h"
#include <string.h>  
#include <windows.h>                       
#include <toolhelp.h>

     

module to_module(const MODULEENTRY& me) { 
	module m;
	m.h = me.hModule;
	m.refcount = me.wcUsage;
	memcpy(m.name, me.szModule, sizeof(me.szModule));
	//memcpy(m.exe_path, me.szExePath, sizeof(me.szExePath));
	return m;	
}   

const module* find(HANDLE h, const module_list& ml) {
	for (int i=0; i<ml.size(); ++i) {
		if (ml[i].h == h) {
			return &(ml[i]);
		}
	}
	return NULL;
}

//
// module_list
//
module_list::module_list() {
	beg_ = NULL;
	last_ = NULL;
	end_ = NULL;
}

module_list::~module_list() {
	delete[] beg_;
}

module& module_list::push_back(const module& m) {
	if (end_ - last_ > 0) {
		*last_ = m;
		++last_;
		return *(last_-1);
	}
		
	// last_ == end_
	reserve(end_-beg_ + 10);
	return push_back(m);
}

int module_list::size() const {
	return last_ - beg_;
}
	
int module_list::capacity() const {
	return end_ - beg_;
}

void module_list::reserve(int newcap) {
	if (newcap <= capacity()) {
		return;
	}
		
	int curr_size = (last_-beg_);
	module* pnew = new module[newcap];
	memcpy(pnew, beg_, curr_size*sizeof(module));
	delete[] beg_;
	beg_ = pnew;
	last_ = beg_+curr_size;
	end_ = beg_+newcap;
}
	
module& module_list::operator[](int n) {
	return *(beg_+n);
}
	
const module& module_list::operator[](int n) const {
	return *(beg_+n);
}








