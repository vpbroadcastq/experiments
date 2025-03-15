## fork12.cpp:  Example from page 12 (Figure 1.7) of APUE
- Mostly C++
- execlp in the child process replaces that process with whatever program the user is trying to execute.<br>
- `clang++ -std=c++20 -O2 fork12.cpp`
<br>

## fcntl84.cpp:  Example from page 84 (Figure 3.11) of APUE
- Converted to C++
- 0 is usually stdin, 1 is usually stdout, 2 is usually stderr
- Ex:  `./a.out 0 < /dev/tty` (/dev/tty is read-only)
- Ex:  `./a.out 5 5<>temp.txt`
- `clang++ -std=c++20 -O2 fcntl184.cpp`

