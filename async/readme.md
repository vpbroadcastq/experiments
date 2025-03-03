
x86-64:
g++ -std=c++20 -g -O2 -fno-exceptions -pthread overload182.cpp
-fsanitize=thread

aarch64 (r pi 5):
clang-19 -std=c++20 -stdlib=libc++
         -I/usr/lib/llvm-19/include/c++/v1 -L/usr/lib/llvm-19/lib
         -lc++ -lc++abi
         -O2 -fno-exceptions -pthread
I had to explictily invoke clang-19 and tell it to use libc++ because piOS is based on debian stable
and debian stable is so obsolete and out of date  that the built-in compilers don't support C++20's
std::format.


objdump -dS -M intel ./a.out | c++filt > disasm.txt

nm -C a.out | grep lambda
-C => demangle

##Observations
###gcc, linux, x86-64
* -The non_atomic_write loop is completely optimized away at -O2, but only when inlined.
 If you put __attribute__((noinline)) on operator(), you get the loop as it is literally
 written.
* relaxed_release_write, relaxed_relaxed_write, non_atomic_write
 All generate essentially the same asm.  When you start using atomic you get padding
 instructions, but otherwise it's just regular mov.
* acquire_relaxed_read, relaxed_relaxed_read, non_atomic_read
 All generate essentially the same asm.  When you start using atomic you get padding
 instructions, but otherwise it's just regular mov.
* -relaxed_sequential_write generates an xchg instruction
* -relaxed_sequential_read looks the same as the other reads
* The seq_cst operation set is a little less than 10x slower on this Celeron N5105.
* Be careful when interpreting indivdual runs; there can be massive differences between runs

###clang, linux, aarch64 (r pi)
* Relaxed store => str, release store => stlr
* Relaxed load => ldr, acquire load => ldar
* Sequential load, store => same as acquire load, release store
