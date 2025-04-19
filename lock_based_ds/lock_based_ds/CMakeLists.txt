# CMakeList.txt : CMake project for lock_based_ds, include source and define
# project specific logic here.
#

# Add source to this project's executable.
add_executable (lock_based_ds "lock_based_ds.cpp" "lock_based_stack.h" "main.cpp" "lock_based_queue.h" "sll_q.h" "map.h")

if (CMAKE_VERSION VERSION_GREATER 3.12)
  set_property(TARGET lock_based_ds PROPERTY CXX_STANDARD 20)
endif()

# TODO: Add tests and install targets if needed.
