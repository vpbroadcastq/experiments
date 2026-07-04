


class Program
{
    private static string NormalizeInputPath(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        if (input == "~" || input.StartsWith("~/"))
        {
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (!string.IsNullOrEmpty(home))
            {
                if (input.Length == 1)
                {
                    return home;
                }
                return Path.Combine(home, input.Substring(2));
            }
        }

        return input;
    }

    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Expected path argument in args[0].");
            return;
        }

        string rootPath = NormalizeInputPath(args[0]);

        //
        // Directory.EnumerateFileSystemEntries
        //
        EnumerationOptions opts = new EnumerationOptions();
        opts.RecurseSubdirectories = true;
        IEnumerable<string> it = Directory.EnumerateFileSystemEntries(rootPath, "*", opts);
        foreach (string curr in it)
        {
            Console.WriteLine(curr);
        }

        Console.WriteLine("--------------------------------------------------------------------------------------------");

        //
        // FsIterator
        //
        FsIterator? fsIt = FsIterator.Create(rootPath);
        if (fsIt == null)
        {
            return;
        }
        while (true)
        {
            string? curr = fsIt.Current();
            if (curr == null)
            {
                break;
            }
            Console.WriteLine(curr);
            fsIt.Next();
        }
    }
}





