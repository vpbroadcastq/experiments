

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

partial class HorribleString
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    private struct HorribleStringImpl
    {
        // Opaque block of memory C++ will interpret as:
        // char*
        // ptrdiff_t
    }
    private HorribleStringImpl impl = default;

    //
    // SMF's
    //
    public HorribleString()
    {
        Console.WriteLine($"C#:  HorribleString.HorribleString()");
        HorribleStringConstruct(ref impl);
    }

    // Deep copy, like you would expect in C++
    public HorribleString(HorribleString rhs)
    {
        Console.WriteLine($"C#:  HorribleString.HorribleString(HorribleString rhs)");
        unsafe
        {
            fixed (HorribleStringImpl* p = &(rhs.impl))
            {
                HorribleStringCopyConstruct(ref impl, (nint)p);
            }
        }
    }

    ~HorribleString()
    {
        Console.WriteLine("C#:  ~HorribleString()");
        HorribleStringDestruct(ref impl);
    }

    // "Move constructor"
    public static HorribleString TakeOwnershipOf(HorribleString rhs)
    {
        Console.WriteLine($"C#:  HorribleString.TakeOwnershipOf(HorribleString rhs)");
        HorribleString result = new HorribleString();
        unsafe
        {
            fixed (HorribleStringImpl* p = &(rhs.impl))
            {
                HorribleStringMoveConstruct(ref result.impl, (nint)p);
            }
        }
        return result;
    }

    public HorribleString(ReadOnlySpan<byte> str)
    {
        Console.WriteLine($"C#:  HorribleString.HorribleString({str.ToString()})");
        unsafe
        {
            fixed (byte* p = str)
            {
                HorribleStringConstructFromString(ref impl, (nint)p);
            }
        }
    }

    //
    // Other
    //
    public unsafe ReadOnlySpan<byte> Get()
    {
        Console.WriteLine($"C#:  HorribleString.Get()");
        nint p = HorribleStringGet(ref impl);
        return MemoryMarshal.CreateReadOnlySpanFromNullTerminated((byte*)p);
    }

    public Int64 Size()
    {
        Console.WriteLine($"C#:  HorribleString.Size()");
        return HorribleStringSize(ref impl);
    }

    public void Set(ReadOnlySpan<byte> str)
    {
        Console.WriteLine($"C#:  HorribleString.Set({str.ToString()})");
        unsafe
        {
            fixed (byte* p = str)
            {
                HorribleStringSet(ref impl, (nint)p);
            }
        }
    }

    public string Getu16String()
    {
        ReadOnlySpan<byte> u8 = Get();
        return Encoding.UTF8.GetString(u8);
    }


    //
    // HorribleString imports
    //
    [LibraryImport("/home/ben/personal/dev/experiments/nativeManagedInterop/native/build/libnatlib.so",
    EntryPoint = "_ZN14HorribleStringC1Ev")]
    // HorribleString::HorribleString()
    private static partial void HorribleStringConstruct(ref HorribleStringImpl impl);

    [LibraryImport("/home/ben/personal/dev/experiments/nativeManagedInterop/native/build/libnatlib.so",
    EntryPoint = "_ZN14HorribleStringC1EPc")]
    // HorribleString::HorribleString(char*)
    private static partial void HorribleStringConstructFromString(ref HorribleStringImpl impl, nint src);

    [LibraryImport("/home/ben/personal/dev/experiments/nativeManagedInterop/native/build/libnatlib.so",
    EntryPoint = "_ZN14HorribleStringC1ERKS_")]
    // HorribleString::HorribleString(HorribleString const&)
    private static partial void HorribleStringCopyConstruct(ref HorribleStringImpl impl, nint src);

    [LibraryImport("/home/ben/personal/dev/experiments/nativeManagedInterop/native/build/libnatlib.so",
    EntryPoint = "_ZN14HorribleStringC1EOS_")]
    // HorribleString::HorribleString(HorribleString&&)
    private static partial void HorribleStringMoveConstruct(ref HorribleStringImpl impl, nint src);

    [LibraryImport("/home/ben/personal/dev/experiments/nativeManagedInterop/native/build/libnatlib.so",
    EntryPoint = "_ZN14HorribleStringD1Ev")]
    // HorribleString::~HorribleString()
    private static partial void HorribleStringDestruct(ref HorribleStringImpl impl);

    [LibraryImport("/home/ben/personal/dev/experiments/nativeManagedInterop/native/build/libnatlib.so",
    EntryPoint = "_ZN14HorribleStringaSERKS_")]
    // HorribleString::operator=(HorribleString const&)
    private static partial nint HorribleStringCopyAssign(ref HorribleStringImpl impl, nint src);

    [LibraryImport("/home/ben/personal/dev/experiments/nativeManagedInterop/native/build/libnatlib.so",
    EntryPoint = "_ZN14HorribleStringaSEOS_")]
    // HorribleString::operator=(HorribleString&&)
    private static partial nint HorribleStringMoveAssign(ref HorribleStringImpl impl, nint src);

    [LibraryImport("/home/ben/personal/dev/experiments/nativeManagedInterop/native/build/libnatlib.so",
    EntryPoint = "_ZN14HorribleString3GetEv")]
    // HorribleString::Get()
    private static partial nint HorribleStringGet(ref HorribleStringImpl impl);

    [LibraryImport("/home/ben/personal/dev/experiments/nativeManagedInterop/native/build/libnatlib.so",
    EntryPoint = "_ZNK14HorribleString3GetEv")]
    // HorribleString::Get() const
    private static partial nint HorribleStringGetConst(ref HorribleStringImpl impl);

    [LibraryImport("/home/ben/personal/dev/experiments/nativeManagedInterop/native/build/libnatlib.so",
    EntryPoint = "_ZNK14HorribleString4SizeEv")]
    // HorribleString::Size() const
    private static partial nint HorribleStringSize(ref HorribleStringImpl impl);

    [LibraryImport("/home/ben/personal/dev/experiments/nativeManagedInterop/native/build/libnatlib.so",
    EntryPoint = "_ZN14HorribleString3SetEPc")]
    // HorribleString::Set(char*)
    private static partial void HorribleStringSet(ref HorribleStringImpl impl, nint src);
}



public class Utility
{

    public static string Tou16String(ReadOnlySpan<byte> u8)
    {
        return Encoding.UTF8.GetString(u8);
    }
}
