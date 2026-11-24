
using System.Runtime.InteropServices;



internal static partial class Nat
{
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
        int result = Nat.add7(1,2,3,4,5,6,7);
        Console.WriteLine($"C#:  add7(1,2,3,4,5,6,7) == {result}");

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

