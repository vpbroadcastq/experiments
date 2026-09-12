



using System.Linq.Expressions;
using System.Runtime.InteropServices;

class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: a_alloc <config-file>");
            return;
        }

        //
        // Config File defining categories and rules
        //
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
        Console.WriteLine($"exclusive rules:\n{Utils.DebugPrintExclusiveRules(cd.Value)}\n");
        Console.WriteLine($"exhaustive rules:\n{Utils.DebugPrintExhaustiveRules(cd.Value)}\n");

        //
        // Config file defining symbols and assigning symbols to categories
        //
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
            foreach (List<int> currRule in cd.Value.rulesExclusive)
            {
                Utils.RuleViolationExclusive vExc = Utils.ViloatesExclusiveRule(CollectionsMarshal.AsSpan(currSym.categories), CollectionsMarshal.AsSpan(currRule));
                if (!vExc.IsEmpty())
                {
                    Console.WriteLine($"\tViolates exclusive rule.  Member of {vExc.idxa}, {vExc.idxb}");
                }
            }

            foreach (List<int> currRule in cd.Value.rulesExhaustive)
            {
                bool violates = Utils.ViloatesExhaustiveRule(CollectionsMarshal.AsSpan(currSym.categories), CollectionsMarshal.AsSpan(currRule));
                if (violates)
                {
                    Console.WriteLine($"\tViolates exhaustive rule.  Not a member of any of {Utils.DebugPrintAsCsv(currRule)}");
                }
            }
        }
        Console.WriteLine("\n\n");

        //
        // etfc report
        //
        if (!File.Exists(args[2]))
        {
            Console.WriteLine($"ETFC file not found: {args[2]}");
            return;
        }
        List<Utils.Asset>? etfc = EtfcImporter.Import(File.ReadAllLines(args[2]));
        if (etfc == null)
        {
            Console.WriteLine("etfc == null");
            return;
        }
        foreach (Utils.Asset a in etfc)
        {
            Console.WriteLine($"{a.symbol}:  {a.value}");
        }
    
    } // Main
}























