cmake_minimum_required(VERSION 3.25)
project(mt_dir_walk LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_VERBOSE_MAKEFILE ON)

add_executable(mt_dir_walk
    main.cpp
    sha256.cpp
    utils.cpp
)
