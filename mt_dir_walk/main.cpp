#include <print>
#include <filesystem>
#include <vector>
#include "sha256.h"
#include "timer.h"
#include "utils.h"
#include "mt_stack.h"



uint64_t single_threaded(const std::filesystem::path& root) {
    namespace fs = std::filesystem;

    uint64_t result {};
    std::vector<std::byte> fdata;
    for (const fs::directory_entry& curr : fs::recursive_directory_iterator(root)) {
        if (!fs::is_regular_file(curr)) {
            continue;
        }

        fdata.resize(0);
        if (!readfile(curr, fdata)) {
            std::print("Error reading {}\n", curr.path().string());
            continue;
        }

        sha256 h = hash_sha256(fdata);
        result += static_cast<uint64_t>(h.data[0]);
    }

    return result;
}


// uint64_t mt_shared_work_q(const std::filesystem::path& root, ) {


int main(int argc, char* argv[]) {
    if (argc != 2) {
        std::print("Usage:  prog-name path\n");
        return 1;
    }

    const std::filesystem::path root(argv[1]);
    if (!std::filesystem::is_directory(root)) {
        std::print("{} is not a directory :(\n", root.string());
        return 1;
    }

    //
    // Single threaded
    //
    std::chrono::steady_clock::duration elapsed {};
    {
        scoped_timer t(elapsed);
        volatile uint64_t result = single_threaded(root);
    }
    std::print("Single threaded:  {:%H:%M:%S}\n\n", elapsed);

    //
    // Shared work queue
    //
    {
        //...
    }


    return 0;
}



