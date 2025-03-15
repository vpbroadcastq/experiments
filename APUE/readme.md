## Example from page 12 (Figure 1.7) of APUE
Undisciplined mishmash of C and C++<br>
Mixing iostreams and cstdio is likely going to cause problems (FIX)<br>
<br>
execlp in the child process replaces that process with whatever program the user is trying to execute.<br>
<br>
clang++ -std=c++20 -O2 fork12.cpp <br>




