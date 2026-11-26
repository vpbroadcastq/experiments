
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;


// Random imports from the native library
internal static partial class Nat
{
    static public bool test()
    {
        return true;
    }

    [LibraryImport("/home/ben/personal/dev/experiments/nativeManagedInterop/native/build/libnatlib.so")]
    public static partial int add7(int a, int b, int c, int d, int e, int f, int g);

    //
    // The "NonTrivial type
    //
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

        //
        // HorribleString
        //
        Console.WriteLine("-------------------------------------------------------------------------");
        Console.WriteLine("\nConstruct hs from a byte string");
        ReadOnlySpan<byte> utf8String = "Here is a sample string that i am going to copy around"u8;
        HorribleString hs = new HorribleString(utf8String);
        Int64 hsSize = hs.Size();
        ReadOnlySpan<byte> hsPayload = hs.Get();
        Console.WriteLine($"Read back hs:  {hsSize} characters,  {Utility.Tou16String(hsPayload)}");

        Console.WriteLine("\nCopy construct hs2 from hs");
        HorribleString hs2 = new HorribleString(hs);
        Int64 hs2Size = hs2.Size();
        ReadOnlySpan<byte> hs2Payload = hs2.Get();
        Console.WriteLine($"Read back {hs2Size} characters,  {Utility.Tou16String(hs2Payload)}");

        Console.WriteLine("\nMove hs2 into hs3");
        HorribleString hs3 = HorribleString.TakeOwnershipOf(hs2);
        Console.WriteLine($"Read hs3:  {hs3.Getu16String()}");
        Console.WriteLine($"Read hs2:  {hs2.Getu16String()}");

        GC.KeepAlive(hs);
        GC.KeepAlive(hs2);
        GC.KeepAlive(hs3);
    }
}

