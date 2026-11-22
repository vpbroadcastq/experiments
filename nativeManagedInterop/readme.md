# Examples and experiments with native<->managed interop

## Building
`>./build`
Invokes `cmake` and `dotnet`

## What
### `/native/...`
Native C++ dynamic library with some sample code.  Trivial and non-trivial types.  Global instance of `GlobalPrinter` class to momitor loading and unloading of the library.  


### `/managed/...`
Managed C# program that attempts o use types and functions from the native library.  As part of its build process, it runs the import validator in `/importValidator/`.  An attempt to import something that does not exist in the source library results in a build break.

`class HorribleString` marshalls the C++ implementation of `HorribleString`, a very simple null-terminated byte string.


### `/importValidator/...`
Uses reflection to inspect all the method imports of whatever managed library is passed in on the command line.  Reads the method names/"entry points" as well as the target library.  Then, loads the target libraries and checks each to ensure that those methods exist.  Assumes that all the liobraries being imported from are native.  This could be imporved on.





