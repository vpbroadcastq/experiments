#pragma once

#include <filesystem>
#include <vector>
#include <span>
#include <cstddef>


//
// sha256
//
struct sha256 {
    std::array<std::byte,32> data {};
};

sha256 hash_sha256(std::span<const std::byte>);


// Read in a file in one shot
bool readfile(const std::filesystem::path& file, std::vector<std::byte>& dest);


// "plain" means not a symlink
bool is_plain_directory(const std::filesystem::directory_entry&);
bool is_plain_regular_file(const std::filesystem::directory_entry&);



