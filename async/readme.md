
g++ -std=c++20 -g -O2 -fno-exceptions -pthread overload182.cpp
-fsanitize=thread

objdump -dS -M intel ./a.out | c++filt > disasm.txt

nm -C a.out | grep lambda
-C => demangle

_Observations_
gcc, linux
-The non_atomic_write loop is completely optimized away at -O2, but only when inlined.
 If you put __attribute__((noinline)) on operator(), you get the loop as it is literally
 written.
-relaxed_release_write, relaxed_relaxed_write, non_atomic_write
 All generate essentially the same asm.  When you start using atomic you get padding
 instructions, but otherwise it's just regular mov.
-acquire_relaxed_read, relaxed_relaxed_read, non_atomic_read
 All generate essentially the same asm.  When you start using atomic you get padding
 instructions, but otherwise it's just regular mov.



