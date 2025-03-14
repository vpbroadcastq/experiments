# CMakeList.txt : CMake project for concepts, include source and define
# project specific logic here.
#

# Add source to this project's executable.
add_executable (concepts "concepts.cpp" )

if (CMAKE_VERSION VERSION_GREATER 3.12)
  set_property(TARGET concepts PROPERTY CXX_STANDARD 20)
endif()

# TODO: Add tests and install targets if needed.
