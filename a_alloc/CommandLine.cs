

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









