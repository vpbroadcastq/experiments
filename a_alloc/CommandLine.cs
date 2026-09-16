


//
// unset && error => Makes no sense?
// !unset && error => Usable as an index but there's a problem with the value
// unset && !error => Default constructed state
// !unset && !error => Usable as an index; value ok
//
// TODO:  Possibly a better way to do this would be two shorts and an int?
// TODO:  Shound the error type be ushort?
// TODO:  No GetErrorIfHasError()
//
using System.Formats.Asn1;

ref struct MaybeIdx
{
    private const uint flgUnset = 1u<<31;
    private const uint flgError = 1u<<30;

    // ff|uuuuuuuuuuuuuu|eeeeeeee'eeeeeeee|vvvvvvvv'vvvvvvvv'vvvvvvvv'vvvvvvvv
    // f ~ flags, e ~ custom error data, v ~ value
    private ulong i = 0 | flgUnset;

    // Unset, no error
    public MaybeIdx()
    {
        //...
    }

    public int Set(int i)
    {
        if (i<0) { throw new IndexOutOfRangeException(); }
        this.i = 0;  // Clear all flags
        this.i = (ulong)i;
        return i;
    }

    public int SetError(int i, short err)
    {
        if (i<0) { throw new IndexOutOfRangeException(); }
        this.i = flgError;
        this.i |= (ulong)err << 32;
        this.i |= (ulong)i;
        return i;
    }

    // "IsSet"
    public bool HasValue()
    {
        return (this.i & flgUnset) != flgUnset;
    }

    public bool HasError()
    {
        return (this.i & flgError) == flgError;
    }

    // Ignores the unset state and the error state
    public int GetUnsafe()
    {
        return (int)(this.i & 0x00000000FFFFFFFFu);
    }

    // There may or may not be an error
    public int? GetIfHasValue()
    {
        if (!HasValue())
        {
            return null;
        }
        return GetUnsafe();
    }

    // Must be both set and have no error
    public int? GetIfNoError()
    {
        if (!HasValue() || HasError())
        {
            return null;
        }
        return GetUnsafe();
    }
    
    public short GetErrorUnsafe()
    {
        return (short) (this.i >> 32);
    }
}


// The getters don't check IsValid or the index because the language is going to bounds check access to the
// args[] array anyhow.  TODO:  What is C# bets practice here?
class CommandLine
{
    private string[] args;
    // -1 means unset
    int idxCats = -1;
    int idxSymbs = -1;
    int idxInputBeg = -1;
    int idxInputEnd = -1;
    string? error = null; // TODO gross :(

    public CommandLine(string[] args)
    {
        this.args = args;

        for (int i=0; i<args.Count(); ++i)
        {
            if (Utils.IsEq(args[i],"--categories"))
            {
                if (idxCats!=-1)
                {
                    error += "More than one --categories command\n";
                }
                else if (i<(args.Count()-1) && !args[i+1].StartsWith("--") && Utils.IsPathValid(args[i+1]))
                {
                    idxCats = i+1;
                }
                else
                {
                    error += "Invalid input for --categories argument\n";
                }
                continue;
            }

            if (Utils.IsEq(args[i],"--symbols"))
            {
                if (idxSymbs!=-1)
                {
                    error += "More than one --symbols command\n";
                }
                else if (i<(args.Count()-1) && !args[i+1].StartsWith("--") && Utils.IsPathValid(args[i+1]))
                {
                    idxSymbs = i+1;
                }
                else
                {
                    error += "Invalid input for --symbols argument\n";
                }
                continue;
            }

            if (idxInputBeg==-1 && Utils.IsEq(args[i],"--input"))
            {
                if (idxInputBeg!=-1)
                {
                    error += "More than one --input command\n";
                    continue;
                }

                for (int j=i+1; j<args.Count(); ++j)
                {
                    if (args[j].StartsWith("--"))
                    {
                        idxInputEnd = j;
                        break; // Not an invalid argument; signals the start of the next command
                    }
                    else if (!Utils.IsPathValid(args[j]))
                    {
                        error += $"Invalid argument in --input command: {args[j]}\n";
                        //break; // Invalid argument
                    }

                    if (idxInputBeg==-1) // first iteration
                    {
                        idxInputBeg = j;
                    }
                    idxInputEnd = j+1;
                }

                if (idxInputBeg == -1)
                {
                    error += "Empty argument list for argument --input\n";
                }
                continue;
            }
        } // To next i in args[]
    }

    public bool IsValid()
    {
        return (error==null) && (idxCats != -1 && idxSymbs != -1 && idxInputBeg != -1 && idxInputEnd != -1);
    }

    public ReadOnlySpan<char> GetError()
    {
        return error;
    }

    public ReadOnlySpan<char> GetCategoriesConfig()
    {
        return args[idxCats];
    }

    public ReadOnlySpan<char> GetSymbolsConfig()
    {
        return args[idxSymbs];
    }

    public int CountDataFiles()
    {
        return idxInputEnd-idxInputBeg;
    }

    public ReadOnlySpan<char> GetDataFile(int i)
    {
        return args[idxInputBeg+i];
    }
};









