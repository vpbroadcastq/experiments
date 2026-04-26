#include "utils.h"

#include <vector>
#include <filesystem>
#include <cstdio>





bool readfile(const std::filesystem::path& file, std::vector<std::byte>& dest) {
    dest.resize(0);
    if (!std::filesystem::is_regular_file(file) || !std::filesystem::exists(file)) {
        // Redundant check?  is_regular_file must => exists, right???
        return false;
    }

    FILE* fp = std::fopen(file.c_str(), "rb");
    if (!fp) {
        return false;
    }

    size_t nbytes = std::filesystem::file_size(file);
    dest.resize(nbytes);
    size_t nbytes_read = std::fread(dest.data(), 1, nbytes, fp);
    std::fclose(fp);

    if (nbytes != nbytes_read) {
        dest.resize(0);
        return false;
    }

    return true;
}

