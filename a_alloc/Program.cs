



class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: a_alloc <config-file>");
            return;
        }

        if (!File.Exists(args[0]))
        {
            Console.WriteLine($"Config file not found: {args[0]}");
            return;
        }

        Utils.ConfigData? cd = Utils.ReadConfig(File.ReadAllLines(args[0]));
        if (cd == null)
        {
            Console.WriteLine("cd == null");
            return;
        }
        Console.WriteLine($"categories:\n{Utils.DebugPrint(cd.Value.categories)}\n\n");
        Console.WriteLine($"exclusive rules:\n{Utils.DebugPrint(cd.Value.rulesExclusive)}\n\n");
        Console.WriteLine($"exhaustive rules:\n{Utils.DebugPrint(cd.Value.rulesExhaustive)}\n\n");
    }
}























