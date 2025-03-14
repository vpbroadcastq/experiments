# CMakeList.txt : CMake project for ts_queue, include source and define
# project specific logic here.
#

# Add source to this project's executable.
add_executable (ts_queue "ts_queue.cpp" "ts_queue.h" "main.cpp")

if (CMAKE_VERSION VERSION_GREATER 3.12)
  set_property(TARGET ts_queue PROPERTY CXX_STANDARD 20)
endif()

# TODO: Add tests and install targets if needed.
