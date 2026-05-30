# Filesystem walking and benchmarks

## Alternate approaches to try
1)  Work-stealing deques.  Give each worker its own local deque of directories and let idle threads steal from others.  Possibly better when subtree sizes are uneven.

2)  Metadata-only mode.  Count files, accumulate path lengths, or hash path names plus sizes without opening file contents. Right now the benchmark is dominated by reading and hashing, so a metadata-only mode would tell you much more about the walker itself.

3)  System-specific low-level walker.  Compare std::filesystem against opendir/readdir or getdents64 (and whatever on Windows) to investigate abstraction overhead.

4)  Same traversal, different file ingestion.  Keep the walk identical but compare read into buffer versus mmap.







