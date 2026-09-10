



using System.Linq.Expressions;

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

        if (!File.Exists(args[1]))
        {
            Console.WriteLine($"Symbols file not found: {args[1]}");
            return;
        }

        List<Utils.Symbol>? syms = Utils.ReadSymbols(File.ReadAllLines(args[1]), cd.Value.categories);
        if (syms == null)
        {
            Console.WriteLine("syms == null");
            return;
        }
        Console.WriteLine("Symbols:");
        foreach (Utils.Symbol currSym in syms)
        {
            Console.WriteLine($"{currSym.symbol}:  {Utils.DebugPrintAsCsv(currSym.categories)}");
        }
        Console.WriteLine("\n\n");
    }
}























