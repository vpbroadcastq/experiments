#pragma once

#include <filesystem>
#include <vector>


bool readfile(const std::filesystem::path& file, std::vector<std::byte>& dest);

// "plain" means not a symlink
bool is_plain_directory(const std::filesystem::directory_entry&);
bool is_plain_regular_file(const std::filesystem::directory_entry&);



