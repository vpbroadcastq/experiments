#include "mt_stack.h"
#include "sha256.h"
#include "timer.h"
#include "utils.h"
#include <atomic>
#include <charconv>
#include <filesystem>
#include <print>
#include <string_view>
#include <thread>
#include <vector>

uint64_t single_threaded(const std::filesystem::path& root) {
    namespace fs = std::filesystem;

    uint64_t result{};
    std::vector<std::byte> fdata;
    for (const fs::directory_entry& curr :
         fs::recursive_directory_iterator(root)) {
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

uint64_t mt_shared_work_q(const std::filesystem::path& root, size_t nthreads) {
    mt_stack<std::filesystem::path> stk;
    std::atomic<bool> pilot_work_completed{false};
    std::atomic<uint64_t> result{};

    // "Pilot" thread walks the filesystem recursively beginning at the root and
    // pushes every directory that it finds onto the stack
    auto pilot = [&stk, &pilot_work_completed, &root]() {
        namespace fs = std::filesystem;
        for (const fs::directory_entry& curr :
             fs::recursive_directory_iterator(root)) {
            if (fs::is_directory(curr)) {
                stk.push(curr);
                //std::print("Pushed {}\n", curr.path().string());
            }
        }
        pilot_work_completed = true;
    };

    // Worker threads pop directories from the stack and walk all the files (non-directories)
    // contained immediately therein and compute the sha256 for each one.
    auto worker = [&stk, &pilot_work_completed, &result]() {
        namespace fs = std::filesystem;

        std::vector<std::byte> fdata;
        while (true) {
            std::optional<fs::path> curr_dir;
            if (pilot_work_completed) {
                curr_dir = stk.pop();
                if (!curr_dir) {
                    break;
                }
            } else {
                curr_dir = stk.pop();
            }
            if (!curr_dir) {
                continue;
            }

            for (const fs::directory_entry curr_file :
                 fs::directory_iterator(*curr_dir)) {
                if (!fs::is_regular_file(curr_file)) {
                    continue;
                }

                fdata.resize(0);

                // Skip anything > 10 Mb
                size_t nbytes = std::filesystem::file_size(curr_file.path());
                if (nbytes > 10'000'000) {
                    continue;
                }

                if (!readfile(curr_file.path(), fdata)) {
                    std::print("Error reading {}\n", curr_file.path().string());
                    continue;
                }
                // std::print("Reading {}\n", curr_file.path().string());

                sha256 h = hash_sha256(fdata);
                result += static_cast<uint64_t>(h.data[0]);
            }
        }
    };

    std::thread t_pilot(pilot);
    t_pilot.detach();

    std::vector<std::thread> workers;
    workers.reserve(nthreads);
    for (size_t i = 0; i < nthreads; ++i) {
        workers.push_back(std::thread(worker));
    }
    for (size_t i = 0; i < workers.size(); ++i) {
        workers[i].join();
    }

    return result;
}

int main(int argc, char* argv[]) {
    if (argc != 3) {
        std::print("Usage:  prog-name path s|m<n>\n");
        std::print("  s      single-threaded\n");
        std::print("  m<n>   multi-threaded with n threads (e.g. m8)\n");
        return 1;
    }

    const std::filesystem::path root(argv[1]);
    if (!std::filesystem::is_directory(root)) {
        std::print("{} is not a directory :(\n", root.string());
        return 1;
    }

    std::string_view mode(argv[2]);

    if (mode == "s") {
        std::chrono::steady_clock::duration elapsed{};
        {
            scoped_timer t(elapsed);
            volatile uint64_t result = single_threaded(root);
        }
        std::print("Single-threaded:  {:%H:%M:%S}\n", elapsed);
    } else if (mode.starts_with('m')) {
        size_t nthreads{};
        const std::string_view n_str = mode.substr(1);
        auto [ptr, ec] = std::from_chars(n_str.data(), n_str.data() + n_str.size(), nthreads);
        if (ec != std::errc{} || nthreads == 0) {
            std::print("Invalid thread count in '{}'. Use e.g. m8\n", mode);
            return 1;
        }
        std::chrono::steady_clock::duration elapsed{};
        {
            scoped_timer t(elapsed);
            volatile uint64_t result = mt_shared_work_q(root, nthreads);
        }
        std::print("Multi-threaded ({} threads):  {:%H:%M:%S}\n", nthreads, elapsed);
    } else {
        std::print("Unknown mode '{}'. Use 's' or 'm<n>' (e.g. m8)\n", mode);
        return 1;
    }

    return 0;
}
