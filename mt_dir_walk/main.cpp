#include <print>
#include <atomic>
#include <thread>
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
    /*std::chrono::steady_clock::duration elapsed {};
    {
        scoped_timer t(elapsed);
        volatile uint64_t result = single_threaded(root);
    }
    std::print("Single threaded:  {:%H:%M:%S}\n\n", elapsed);*/

    //
    // Shared work queue populated by a "runner thread" that walks the fs recursively and
    // pushes all _directories_ onto the stack.  Worker threads pop from the stack and hash
    // the non-directories contained in each entry.
    //
    {
        std::chrono::steady_clock::duration elapsed {};

        constexpr int n_threads {8};
        mt_stack<std::filesystem::path> stk;
        std::atomic<bool> pilot_work_completed {false};
        std::atomic<uint64_t> result {};

        auto pilot = [&stk, &pilot_work_completed, &root]() {
            namespace fs = std::filesystem;
            for (const fs::directory_entry& curr : fs::recursive_directory_iterator(root)) {
                if (fs::is_directory(curr)) {
                    stk.push(curr);
                    //std::print("Pushed {}\n", curr.path().string());
                }
            }
            pilot_work_completed = true;
        };

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

                for (const fs::directory_entry curr_file: fs::directory_iterator(*curr_dir)) {
                    if (!fs::is_regular_file(curr_file)) {
                        continue;
                    }

                    fdata.resize(0);
                    if (!readfile(curr_file.path(), fdata)) {
                        std::print("Error reading {}\n", curr_file.path().string());
                        continue;
                    }
                    //std::print("Reading {}\n", curr_file.path().string());

                    sha256 h = hash_sha256(fdata);
                    result += static_cast<uint64_t>(h.data[0]);
                }
            }
        };

        {
            scoped_timer t(elapsed);
            std::thread t_pilot(pilot);
            t_pilot.detach();

            std::array<std::thread, n_threads> workers;
            for (int i=0; i<n_threads; ++i) {
                workers[i] = std::thread(worker);
            }
            for (int i=0; i<n_threads; ++i) {
                workers[i].join();
            }
        }

        std::print("Multi-threaded:  {:%H:%M:%S}\n\n", elapsed);
    }


    return 0;
}



