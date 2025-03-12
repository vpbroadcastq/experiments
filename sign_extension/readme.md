## To build (linux):<br>
nasm -f elf64 -o movlib.lib movlib_lin.nasm<br>
g++ -c -std=c++20 -O2 main.cpp<br>
g++ -o main.exe main.o movlib.lib<br>
<br>
## To build (Windows):<br>
nasm -f win64 -o movlib.obj .\movlib_win.nasm<br>
cl /std:c++20 /c /EHsc- /O2 /Fo"main.obj" main.cpp<br>
link main.obj movlib.obj /OUT:"main.exe"<br>
<br>
<br>
Note the -f argument to nasm in both cases.<br>


