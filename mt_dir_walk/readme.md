# mt_dir_walk
Small experiment for comparing recursive directory traversal strategies.  Symlinks are ignored.

## Configure and build (CMake)
From the project root:
```bash
cmake -S . -B build-debug -DCMAKE_BUILD_TYPE=Debug
cmake --build build-debug -j
```
or
```bash
cmake -S . -B build-release -DCMAKE_BUILD_TYPE=RelWithDebInfo
cmake --build build-release -j
```

Or use the existing VS Code task `Configure and build Debug`.

You can also use the project build helper script:
```bash
./build debug
./build rel
./build clean
```

## Run
From the project root, run the debug binary with:
```bash
./build-debug/mt_dir_walk <path> <mode>
```

Modes:
- `s`: single-threaded traversal
- `m<n>`: multi-threaded no-pilot traversal with max `<n>` threads (example: `m8`)
- `p<n>`: multi-threaded pilot-thread traversal with `<n>` worker threads (example: `p8`)

Examples:
```bash
./build-debug/mt_dir_walk ./test s
./build-debug/mt_dir_walk ./test m8
./build-debug/mt_dir_walk ./test p8
```
