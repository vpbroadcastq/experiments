cmake_minimum_required(VERSION 4.3.0)
project(NativeCodeDynamicLibrary LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 26)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)
add_compile_options(-fno-exceptions)

add_library(natlib SHARED
    lib.cpp)


