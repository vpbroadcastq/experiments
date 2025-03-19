# CMakeList.txt : CMake project for compare_exchange, include source and define
# project specific logic here.
#

# Add source to this project's executable.
add_executable (compare_exchange "compare_exchange.cpp" "compare_exchange.h")

if (CMAKE_VERSION VERSION_GREATER 3.12)
  set_property(TARGET compare_exchange PROPERTY CXX_STANDARD 20)
endif()

# TODO: Add tests and install targets if needed.
