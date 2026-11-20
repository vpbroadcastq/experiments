

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal class NatLib
{
    private readonly nint m_h;

    public NatLib(string path)
    {
        m_h = NativeLibrary.Load(path);
    }

    ~NatLib()
    {
        NativeLibrary.Free(m_h);
    }

    public bool ExportExists(string name)
    {
        nint address = 0;
        bool b = NativeLibrary.TryGetExport(m_h, name.ToString(), out address);
        return b;
    }
}


