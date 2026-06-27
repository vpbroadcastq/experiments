#include "mt_stack.h"
#include "timer.h"
#include "utils.h"
#include <atomic>
#include <charconv>
#include <filesystem>
#include <print>
#include <string_view>
#include <thread>
#include <vector>



//
// Traversal strategies
//
// uint64_t single_threaded(const std::filesystem::path& root)
// uint64_t single_shared_q_no_pilot(const std::filesystem::path& root, size_t max_nthreads)
// uint64_t single_shared_q_pilot_thread(const std::filesystem::path& root, size_t nthreads)
//
//
//
//




//
// Single-threaded recursive directory walk
//
uint64_t single_threaded(const std::filesystem::path& root) {
    namespace fs = std::filesystem;

    uint64_t result{};
    //std::vector<std::byte> fdata;
    for (const fs::directory_entry& curr : fs::recursive_directory_iterator(root, fs::directory_options::skip_permission_denied)) {
        if (!is_plain_regular_file(curr)) {
            continue;
        }

        // Skip anything > 10 Mb
        /*size_t nbytes = std::filesystem::file_size(curr.path());
        if (nbytes > 10'000'000) {
            continue;
        }

        fdata.resize(0);
        if (!readfile(curr, fdata)) {
            std::print("Error reading {}\n", curr.path().string());
            continue;
        }

        sha256 h = hash_sha256(fdata);
        result += static_cast<uint64_t>(h.data[0]);*/

        result += fs::file_size(curr);
    }

    return result;
}


//
// The design here is fragile.  
// Has to be move ctorable, otherwise you get a copy when passing the temporary into std::thread, and the
// implictly-defined copy ctor does not increment nthreads.
// Move operations also have to remember to clear out nthreads from the source object lest you get a double
// decrement.
//
struct fully_parallel_worker {
    mt_stack<std::filesystem::path>* stk {};
    std::atomic<uint64_t>* result {};
    std::atomic<int32_t>* nthreads {};
    std::atomic<bool>* creating_thread_flag {};
    const uint64_t max_nthreads;

    fully_parallel_worker(mt_stack<std::filesystem::path>& stk, std::atomic<uint64_t>& result,
        std::atomic<int32_t>& nthreads_, std::atomic<bool>& creating_thread_flag_, uint64_t max_nthreads)
        : stk(&stk), result(&result), nthreads(&nthreads_), creating_thread_flag(&creating_thread_flag_),
          max_nthreads(max_nthreads) {
            // Note that the nthreads increment occurs in the ctor.  It matters that the _creating_ thread
            // increment the count rather than the _created_ thread because there is no knowing when the
            // _created_ thread will actually begin to run.
            nthreads->fetch_add(1);
    }

    fully_parallel_worker(fully_parallel_worker&& lhs) : max_nthreads(lhs.max_nthreads) {
        this->stk = lhs.stk;
        this->result = lhs.result;
        this->nthreads = lhs.nthreads;
        this->creating_thread_flag = lhs.creating_thread_flag;
        lhs.nthreads = nullptr;  // Note!
    }

    fully_parallel_worker(const fully_parallel_worker&) = delete;
    fully_parallel_worker& operator=(const fully_parallel_worker&) = delete;
    fully_parallel_worker& operator=(fully_parallel_worker&&) = delete;  // Can exist but needs to null nthreads

    ~fully_parallel_worker() {
        if (!nthreads) {  // null in a moved-from object
            return;
        }

        int32_t prev = nthreads->fetch_sub(1);
        if (prev == 1) {
            nthreads->notify_all();
        }
    }


    //
    // Pops directories off the shared stack and iterates over the entries in that directory.  When it encounters
    // an entry that is a folder, adds it to the stk and attempts to create a new thread.
    //
    void operator()() {
        namespace fs = std::filesystem;
        //std::vector<std::byte> fdata;

        std::optional<fs::path> curr {};
        while (true) {
            curr = stk->pop();
            if (!curr) {
                return;
            }

            for (const fs::directory_entry& de : fs::directory_iterator(*curr, fs::directory_options::skip_permission_denied)) {
                if (is_plain_directory(de)) {
                    stk->push(de);
                    bool expected {false};
                    if (creating_thread_flag->compare_exchange_strong(expected, true)) {
                        if (*nthreads < max_nthreads) {
                            std::thread t(fully_parallel_worker(*stk, *result, *nthreads, *creating_thread_flag, max_nthreads));
                            t.detach();
                        }
                        *creating_thread_flag = false;
                    }  // else { someone is creating a thread rn... don't even bother to check the count
                    continue;
                }

                if (!is_plain_regular_file(de)) {
                    continue;
                }

                /*fdata.resize(0);

                // Skip anything > 10 Mb
                size_t nbytes = fs::file_size(de.path());
                if (nbytes > 10'000'000) {
                    continue;
                }

                if (!readfile(de.path(), fdata)) {
                    std::print("Error reading {}\n", de.path().string());
                    continue;
                }
                //std::print("Reading {}\n", de.path().string());

                sha256 h = hash_sha256(fdata);
                *result += static_cast<uint64_t>(h.data[0]);*/
                *result += fs::file_size(de);
            }
        }
    }  // pop next off stk
};


//
// Unlike the pilot thread impl, in which the "harness" creates the pilot and then a fixed number of worker
// threads, here, the harness creates a single worker thread, and then it and subsequently created worker threads
// are responsible for spawning yet more workers until the maximum has been reached.
//
uint64_t single_shared_q_no_pilot(const std::filesystem::path& root, size_t max_nthreads) {
    mt_stack<std::filesystem::path> stk;
    std::atomic<uint64_t> result{};
    std::atomic<int32_t> nthreads{};
    std::atomic<bool> creating_thread_flag {false};  // Set when somebody is spawning a new thread

    stk.push(root);
    std::thread t(fully_parallel_worker(stk, result, nthreads, creating_thread_flag, max_nthreads));
    t.detach();
    while (true) {
        const int32_t nthreads_old = nthreads;
        if (nthreads == 0) {
            break;
        }
        nthreads.wait(nthreads_old);
    }

    return result;
}


//
// Single "pilot" thread that recursively populates a "queue" of directories, and multiple worker threads
// that pop entries from this queue and iterate over the non-directory contents computing the sha256.
//
// Notes
// - One crappy thing about this design is that the workers spin.  If something delays the pilot
//   thread for some bizarre reason, the workers might consume cpu.
//
uint64_t single_shared_q_pilot_thread(const std::filesystem::path& root, size_t nthreads) {
    mt_stack<std::filesystem::path> stk;
    std::atomic<bool> pilot_work_completed{false};
    std::atomic<uint64_t> result{};

    // "Pilot" thread walks the filesystem recursively beginning at the root and
    // pushes every directory that it finds onto the stack
    auto pilot = [&stk, &pilot_work_completed, &root]() {
        namespace fs = std::filesystem;
        if (!fs::is_directory(root)) {
            pilot_work_completed = true;
            return;
        }

        stk.push(root);
        for (const fs::directory_entry& curr : fs::recursive_directory_iterator(root, fs::directory_options::skip_permission_denied)) {
            if (is_plain_directory(curr)) {
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

        //std::vector<std::byte> fdata;
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

            for (const fs::directory_entry curr_file : fs::directory_iterator(*curr_dir, fs::directory_options::skip_permission_denied)) {
                if (!is_plain_regular_file(curr_file)) {
                    continue;
                }

                /*fdata.resize(0);

                // Skip anything > 10 Mb
                size_t nbytes = std::filesystem::file_size(curr_file.path());
                if (nbytes > 10'000'000) {
                    continue;
                }

                if (!readfile(curr_file.path(), fdata)) {
                    std::print("Error reading {}\n", curr_file.path().string());
                    continue;
                }
                //std::print("Reading {}\n", curr_file.path().string());

                sha256 h = hash_sha256(fdata);
                result += static_cast<uint64_t>(h.data[0]);*/
                result += fs::file_size(curr_file);
            }
        }
    };

    // t_pilot can not outlive scope exit and access a destroyed 'stk' because the worker threads
    // won't join until it sets pilot_work_completed, and once it has set this, it no longer attempts
    // to access stk.
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
        std::print("Usage:  prog-name path s|m<n>|p<n>\n");
        std::print("  s      single-threaded\n");
        std::print("  m<n>   multi-threaded no-pilot with max n threads (e.g. m8)\n");
        std::print("  p<n>   multi-threaded pilot-thread strategy with n workers (e.g. p8)\n");
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
        uint64_t result {};
        {
            scoped_timer t(elapsed);
            result = single_threaded(root);
        }
        std::print("Single-threaded:  {:%H:%M:%S}\nresult = {}\n", elapsed,result);
    } else if (mode.starts_with('m') || mode.starts_with('p')) {
        const bool use_pilot = mode.starts_with('p');
        size_t nthreads{};
        const std::string_view n_str = mode.substr(1);
        auto [ptr, ec] = std::from_chars(n_str.data(), n_str.data() + n_str.size(), nthreads);
        if (ec != std::errc{} || nthreads == 0) {
            std::print("Invalid thread count in '{}'. Use e.g. m8 or p8\n", mode);
            return 1;
        }
        std::chrono::steady_clock::duration elapsed{};
        uint64_t result {};
        {
            scoped_timer t(elapsed);
            result = use_pilot ?
                single_shared_q_pilot_thread(root, nthreads) :
                single_shared_q_no_pilot(root, nthreads);
        }
        std::print("Multi-threaded {} ({} threads):  {:%H:%M:%S}\nresult = {}\n",
            use_pilot ? "pilot" : "no-pilot", nthreads, elapsed, result);
    } else {
        std::print("Unknown mode '{}'. Use 's', 'm<n>', or 'p<n>' (e.g. m8, p8)\n", mode);
        return 1;
    }

    return 0;
}
