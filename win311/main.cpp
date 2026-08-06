#include <string.h> 
#include <stdio.h>
#include <io.h> //_wsetscreenbuf
#include <windows.h>
#include <toolhelp.h>
#include "util.h"
#include "module.h"                                              
                                              
               
                                         

const char* global_type_to_string(WORD t) {
	if (t == GT_DGROUP) {
		return "DGROUP";
	} else if (t == GT_DATA) {
		return "DATA";
	} else if (t == GT_CODE) {
		return "CODE";
	} else if (t == GT_TASK) {
		return "TASK";
	} else if (t == GT_RESOURCE) {
		return "RESOURCE";
	} else if (t == GT_MODULE) {
		return "MODULE";
	} else if (t == GT_FREE) {
		return "FREE";
	} else if (t == GT_INTERNAL) {
		return "INTERNAL";
	} else if (t == GT_SENTINEL) {
		return "SENTINEL";
	}
	return "UNKNOWN";
}

int main(int argc, char* argv[]) {
	_wsetscreenbuf(_fileno(stdout), 64000L); // QuickWin has a stupid small output buffer by default

	// Print loaded modules and populate the module_list ml
	printf("########## MODULES ##########\n");
	module_list ml;
	MODULEENTRY me;
	me.dwSize = sizeof(me);
	if (ModuleFirst(&me)) {
		printf("%-12s   %-32s   %-7s   %-8s\n", "Module", "Exe path", "hModule", "refcount");
		while (1) {
			printf("%-12s   %-32s   %-7x   %-8d\n", me.szModule, me.szExePath, me.hModule, me.wcUsage);
			ml.push_back(to_module(me));
			if (!ModuleNext(&me)) { break; }
		}
	}
	printf("*** Found %d modules total\n\n\n", ml.size());

	
	printf("########## TASKS ##########\n");
	int num_tasks = 0;
	TASKENTRY te;
	te.dwSize = sizeof(te);
	if (TaskFirst(&te)) {
		printf("%-6s   %-12s   %-8s   %-12s\n", "hTask", "hTaskParent", "hModule", "Module");
		while (1) {
			++num_tasks;
			printf("%-6x   %-12x   %-8x   %-12s\n", te.hTask, te.hTaskParent, te.hModule, te.szModule);
			if (!TaskNext(&te)) { break; }
		}
	}
	printf("*** Found %d tasks total\n\n\n", num_tasks);
	

	printf("########## GLOBAL HEAP ##########\n");
	int tot_num_glob_entries = 0;
	int tot_num_glob_entries_identified = 0; // the number mapped to a loaded module
	GLOBALENTRY ge;
	ge.dwSize = sizeof(ge);
	if (GlobalFirst(&ge, GLOBAL_ALL)) {
		printf("%-8s   %-7s   %-7s   %-7s   %-12s    %-7s   %-10s   %-7s\n",
			"Address", "hBlock", "Size", "nLocks", "heapPresent", "hOwner", "Type", "Module");
		while (1) {
			++tot_num_glob_entries;
			const module* pmod = find(ge.hOwner, ml);
			if (pmod) {
				++tot_num_glob_entries_identified;
			}
			printf("%-8lx   %-7x   %-7lx   %-7d   %-12d    %-7x   %-10s   %-7s\n",
				ge.dwAddress, ge.hBlock, ge.dwBlockSize, ge.wcLock, ge.wHeapPresent>0, ge.hOwner,
				global_type_to_string(ge.wType), pmod ? pmod->name : "Unknown");
			if (!GlobalNext(&ge, GLOBAL_ALL)) { break; }
		}
	}
	printf("*** Found %d global entries; %d could be mapped to modules\n\n\n",
		tot_num_glob_entries, tot_num_glob_entries_identified);

	return 0;
}	


                                  