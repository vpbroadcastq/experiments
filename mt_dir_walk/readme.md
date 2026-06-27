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

## Run
From the project root, run the debug binary with:
```bash
./build-debug/mt_dir_walk <path> <mode>
```

Modes:
- `s`: single-threaded traversal
- `m<n>`: multi-threaded traversal with `<n>` threads (example: `m8`)

Examples:
```bash
./build-debug/mt_dir_walk ./test s
./build-debug/mt_dir_walk ./test m8
```
