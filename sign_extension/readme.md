To build (linux):
nasm -f elf64 -o movlib.lib movlib.nasm
g++ -c -std=c++20 -O2 main.cpp
g++ -o main.exe main.o movlib.lib

Note the -f argument to nasm.


