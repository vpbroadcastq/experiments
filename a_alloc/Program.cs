



using System.Linq.Expressions;
using System.Runtime.InteropServices;

class Program
{
    public static void Main(string[] args)
    {
        CommandLine cmdLn = new CommandLine(args);
        if (!cmdLn.IsValid())
        {
            Console.WriteLine(cmdLn.GetError());
            Console.WriteLine("Usage: a_alloc <config-file>");
            return;
        }




        /*List<Segment> segs = new List<Segment> {
            new Segment("haha",0.45),
            new Segment("haha",0.05),
            new Segment("haha",0.20),
            new Segment("haha",0.15),
            new Segment("haha",0.05),
            new Segment("haha",0.10)
        };
        PieChart pc = PieChart.Create(new PieChartConfig(), CollectionsMarshal.AsSpan(segs));
        Console.WriteLine(pc.ToXml());*/

        //
        // Config File defining categories and rules
        //
        if (!File.Exists(cmdLn.GetCategoriesConfig().ToString()))
        {
            Console.WriteLine($"Config file not found: {args[0]}");
            return;
        }

        Utils.ConfigData? cd = Utils.ReadConfig(File.ReadAllLines(cmdLn.GetCategoriesConfig().ToString()));
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
        if (!File.Exists(cmdLn.GetSymbolsConfig().ToString()))
        {
            Console.WriteLine($"Symbols file not found: {cmdLn.GetSymbolsConfig().ToString()}");
            return;
        }

        List<Utils.Symbol>? syms = Utils.ReadSymbols(File.ReadAllLines(cmdLn.GetSymbolsConfig().ToString()), cd.Value.categories);
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


        Func<string[], List<Utils.Asset>?>[] importers = [
            EtfcImporter.Import,
            FidelityImporter.Import,
            TiaaImporter.Import
        ];

        List<Utils.Asset> allAssets = new List<Utils.Asset>();
        for (int i=0; i<cmdLn.CountDataFiles(); ++i)
        {
            ReadOnlySpan<char> currDataFile = cmdLn.GetDataFile(i);
            string[] currFileData = File.ReadAllLines(currDataFile.ToString());
            foreach (var currImporter in importers)
            {
                List<Utils.Asset>? currAssets = currImporter(currFileData);
                if (currAssets != null)
                {
                    Utils.Merge(allAssets,currAssets);
                    break;
                }
            }
        }

        double total = 0.0;
        foreach (Utils.Asset a in allAssets)
        {
            Console.WriteLine($"{a.symbol}:  {a.value}");
            total += a.value;
        }
        Console.WriteLine($"TOTAL:  {total}\n");

        
    
    } // Main

}























