


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
        HashSet<string> hs1 = new HashSet<string>();
        EnumerationOptions opts = new EnumerationOptions();
        opts.RecurseSubdirectories = true;
        IEnumerable<string> it = Directory.EnumerateFileSystemEntries(rootPath, "*", opts);
        foreach (string curr in it)
        {
            Console.WriteLine(curr);
            hs1.Add(curr);
        }

        Console.WriteLine("--------------------------------------------------------------------------------------------");

        //
        // FsIterator
        //
        HashSet<string> hs2 = new HashSet<string>();
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
            hs2.Add(curr);
            fsIt.Next();
        }


        Console.WriteLine("--------------------------------------------------------------------------------------------");
        bool eq = hs1==hs2;
        if (eq)
        {
            Console.WriteLine("Equal");
        }
        else
        {
            Console.WriteLine("Not equal");
            var hs3 = hs2.Except(hs1);
            foreach(string curr in hs3)
            {
                Console.WriteLine(curr);
            }
        }

    }
}





