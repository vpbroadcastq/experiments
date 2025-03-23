
use std::ffi::CStr;
use std::os::raw::c_int;
use std::os::raw::c_char;
// alt: use std::ffi::c_char... what's the diff?


// Debian => /lib/x86_64-linux-gnu/libc.so.6
// FreeBSD => /lib/libc.so.7
#[link(name="c")]
extern {
    pub fn getpid() -> c_int;
    pub fn getppid() -> c_int;
    pub fn getuid() -> c_int;
    pub fn geteuid() -> c_int;
    pub fn getgid() -> c_int;
    pub fn getegid() -> c_int;

    static environ: *const *const c_char;
}

fn main() {
    println!("***** Process identifiers *****");
    unsafe {
        let pid = getpid();
        print!("Process ID getpid()={}\n",pid);
        println!("Process ID of parent getppid()={}",getppid());
        println!("Real user ID getuid()={}",getuid());
        println!("Effective user ID geteuid()={}",geteuid());
        println!("Real group ID getgid()={}",getgid());
        println!("Effective group ID getegid()={}",getegid());
    }

    // environ
    unsafe {
        let mut p = environ;
        while !p.is_null() && !(*p).is_null() {
            let curr = CStr::from_ptr(*p);
            println!("{:?}",curr);
            p = p.wrapping_add(1);
        }
    }
}





