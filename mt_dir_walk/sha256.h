#pragma once

#include <array>
#include <cstddef>
#include <span>


struct sha256 {
    std::array<std::byte,32> data {};
};

sha256 hash_sha256(std::span<const std::byte>);


