
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;



internal static partial class Nat
{
    static public bool test()
    {
        return true;
    }

    [LibraryImport("/home/ben/personal/dev/experiments/nativeManagedInterop/native/build/libnatlib.so")]
    public static partial int add7(int a, int b, int c, int d, int e, int f, int g);

    [StructLayout(LayoutKind.Explicit, Size=32)]
    public unsafe struct NonTrivial
    {
        //...
    }

    [LibraryImport("/home/ben/personal/dev/experiments/nativeManagedInterop/native/build/libnatlib.so")]
    public static partial NonTrivial MakeNonTrivial();

    [LibraryImport("/home/ben/personal/dev/experiments/nativeManagedInterop/native/build/libnatlib.so",
    EntryPoint = "_ZN10NonTrivial6GetPtrEv")]
    public static partial nint GetPtr(ref NonTrivial nt);
}


class Program
{
    private static readonly Stream Stdout = Console.OpenStandardOutput();

    static void Main()
    {
        Console.WriteLine("C#:  Hello, World!");

        //
        // Symbol resolution
        //
        {
            // Check the native library for a few symbols
            NatLib nl = new NatLib("/home/ben/personal/dev/experiments/nativeManagedInterop/native/build/libnatlib.so");
            bool add7_exists = nl.ExportExists("add7");
            Console.WriteLine($"add7_exists=={add7_exists}");
            bool getptr_exists = nl.ExportExists("_ZN10NonTrivial6GetPtrEv");
            Console.WriteLine($"getptr_exists=={getptr_exists}");
            bool bogus_exists = nl.ExportExists("SomethingThatDoesNotExistLaLaLa");
            Console.WriteLine($"bogus_exists=={bogus_exists}");


            // Reflect over the static class Nat and determine which methods it tries to import from the native
            // library
            MethodInfo[] importMethods = typeof(Nat).GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static);
            foreach (MethodInfo mi in importMethods)
            {
                // Print anything that has a LibraryImportAttribute
                LibraryImportAttribute? a = mi.GetCustomAttribute<LibraryImportAttribute>();
                if (a == null)
                {
                    continue;
                }
                string? ep = a.EntryPoint;
                Console.WriteLine($"\tImported method:  {mi.Name} => {ep}");
            }
        }

        //
        // Simple function calls
        //
        int result = Nat.add7(1,2,3,4,5,6,7);
        Console.WriteLine($"C#:  add7(1,2,3,4,5,6,7) == {result}");


        //
        // Marshalling types
        //
        Nat.NonTrivial nt = Nat.MakeNonTrivial();
        nint p = Nat.GetPtr(ref nt);
        //string? str = Marshal.PtrToStringUTF8(p);
        //Console.WriteLine($"C#:  {str}");
        unsafe
        {
            ReadOnlySpan<byte> utf8 = MemoryMarshal.CreateReadOnlySpanFromNullTerminated((byte*)p);
            Stdout.Write(utf8);
            Console.WriteLine();
        }
    }
}

