Compile with `rustc procenv.rs`



The program getpid.cpp was used to termine where getpid() actually was

`clang++ -std=c++20` to generate `a.out`, then

`ldd a.out` to list the libraries it links against, then, one at a time,

`nm -DC /path/to/lib | grep getpid` (-C=>demangle, -D=>only display dynamic symbols).

On FreeBSD, it's` /lib/libc.so.7` which apparently you reference with `name="c"`






