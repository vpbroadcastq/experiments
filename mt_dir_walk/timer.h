#pragma once

#include <chrono>

// Records elapsed wall-clock time into `elapsed` on destruction.
//
// Format string for hh:mm:ss:ms (requires <format> and <chrono>):
//   std::format("{:%H:%M:%S}", elapsed)
// %S in std::chrono formatting includes sub-second precision automatically;
// for an explicit millisecond field use the duration decomposition:
//   std::format("{:02}:{:02}:{:02}:{:03}",
//       duration_cast<hours>(elapsed).count() % 24,
//       duration_cast<minutes>(elapsed).count() % 60,
//       duration_cast<seconds>(elapsed).count() % 60,
//       duration_cast<milliseconds>(elapsed).count() % 1000)

class scoped_timer {
    using clock      = std::chrono::steady_clock;
    using duration   = clock::duration;

    clock::time_point start_;
    duration&         elapsed_;

public:
    explicit scoped_timer(duration& elapsed)
        : start_(clock::now()), elapsed_(elapsed) {}

    ~scoped_timer() {
        elapsed_ = clock::now() - start_;
    }

    scoped_timer(const scoped_timer&)            = delete;
    scoped_timer& operator=(const scoped_timer&) = delete;
};
