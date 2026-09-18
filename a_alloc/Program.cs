



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
        Console.WriteLine($"TOTAL:  {total}\n\n");

        {
            //
            // Equity vs Fixed income & cash
            //
            int catIdxEquity = cd.Value.categories.IndexOf("equity");
            int catIdxFi = cd.Value.categories.IndexOf("fixed_income");
            int catIdxCash = cd.Value.categories.IndexOf("cash");
            double equityTotal = 0.0;
            double fiTotal = 0.0;
            foreach (Utils.Asset a in allAssets)
            {
                int symIdx = syms.FindIndex(s => Utils.IsEq(s.symbol,a.symbol));
                if (symIdx == -1)
                {
                    Console.WriteLine($"Error!  Unable to find {a.symbol} in the symbol list\n\n");
                    return;
                }
                
                if (syms[symIdx].InCat(catIdxEquity))
                {
                    equityTotal += a.value;
                }
                else if (syms[symIdx].InCat(catIdxFi) || syms[symIdx].InCat(catIdxCash))
                {
                    fiTotal += a.value;
                }
                else
                {
                    Console.WriteLine($"Error!  {a.symbol} not either Fi/Cash or Equity???\n\n");
                    return;
                }
            }
            Console.WriteLine($"equityTotal+fiTotal = {equityTotal} + {fiTotal} = {equityTotal + fiTotal}\n\n");

            double fracEquity = equityTotal/total;
            double fracFi = fiTotal/total;
            List<Segment> segs = new List<Segment> {
                new Segment("Fixed income",fracFi),
                new Segment("Equity",fracEquity)
            };
            PieChart pc = PieChart.Create(new PieChartConfig(), CollectionsMarshal.AsSpan(segs));
            Console.WriteLine(pc.ToXml());
            Console.WriteLine("\n\n");
            File.WriteAllText("rqfi.svg", pc.ToXml());
        }

        {
            //
            // Equity foreign vs domestic
            //
            int catIdxEquity = cd.Value.categories.IndexOf("equity");
            int catIdxForeign = cd.Value.categories.IndexOf("foreign");
            int catIdxDomestic = cd.Value.categories.IndexOf("domestic");
            double equityTotal = 0.0;
            double equityForeignTotal = 0.0;
            double equityDomesticTotal = 0.0;
            foreach (Utils.Asset a in allAssets)
            {
                int symIdx = syms.FindIndex(s => Utils.IsEq(s.symbol,a.symbol));
                if (symIdx == -1)
                {
                    Console.WriteLine($"Error!  Unable to find {a.symbol} in the symbol list\n\n");
                    return;
                }
                
                if (syms[symIdx].InCat(catIdxEquity))
                {
                    equityTotal += a.value;
                }
                else
                {
                    continue;
                }

                if (syms[symIdx].InCat(catIdxForeign))
                {
                    equityForeignTotal += a.value;
                }
                else if (syms[symIdx].InCat(catIdxDomestic))
                {
                    equityDomesticTotal += a.value;
                }
                else
                {
                    Console.WriteLine($"Error!  Equity-categorized {a.symbol} not either foreign or domestic???\n\n");
                    return;
                }
            }
            Console.WriteLine($"equityForeignTotal + EquityDomesticTotal = {equityForeignTotal} + {equityDomesticTotal} = {equityTotal}\n\n");

            double fracForeign = equityForeignTotal/equityTotal;
            double fracDomestic = equityDomesticTotal/equityTotal;
            List<Segment> segs = new List<Segment> {
                new Segment("Foreign",fracForeign),
                new Segment("Domestic",fracDomestic)
            };
            PieChart pc = PieChart.Create(new PieChartConfig(), CollectionsMarshal.AsSpan(segs));
            Console.WriteLine(pc.ToXml());
            Console.WriteLine("\n\n");
            File.WriteAllText("eqdi.svg", pc.ToXml());
        }

        {
            //
            // Equity by geography and capitalization
            //
            int catIdxEquity = cd.Value.categories.IndexOf("equity");
            int catIdxForeign = cd.Value.categories.IndexOf("foreign");
            int catIdxDomestic = cd.Value.categories.IndexOf("domestic");
            int catIdxSmall = cd.Value.categories.IndexOf("small");
            int catIdxMid = cd.Value.categories.IndexOf("mid");
            int catIdxLarge = cd.Value.categories.IndexOf("large");
            double equityTotal = 0.0;
            double foreignSmallTotal = 0.0;
            double foreignMidTotal = 0.0;
            double foreignLargeTotal = 0.0;
            double domesticSmallTotal = 0.0;
            double domesticMidTotal = 0.0;
            double domesticLargeTotal = 0.0;
            foreach (Utils.Asset a in allAssets)
            {
                int symIdx = syms.FindIndex(s => Utils.IsEq(s.symbol,a.symbol));
                if (symIdx == -1)
                {
                    Console.WriteLine($"Error!  Unable to find {a.symbol} in the symbol list\n\n");
                    return;
                }
                
                if (syms[symIdx].InCat(catIdxEquity))
                {
                    equityTotal += a.value;
                }
                else
                {
                    continue;
                }

                if (syms[symIdx].InCat(catIdxForeign))
                {
                    if (syms[symIdx].InCat(catIdxSmall))
                    {
                        foreignSmallTotal += a.value;
                    }
                    else if (syms[symIdx].InCat(catIdxMid))
                    {
                        foreignMidTotal += a.value;
                    }
                    else if (syms[symIdx].InCat(catIdxLarge))
                    {
                        foreignLargeTotal += a.value;
                    }
                    else
                    {
                        Console.WriteLine($"Error!  Foreign equity {a.symbol} not small, mid, or large???\n\n");
                        return;
                    }
                }
                else if (syms[symIdx].InCat(catIdxDomestic))
                {
                    if (syms[symIdx].InCat(catIdxSmall))
                    {
                        domesticSmallTotal += a.value;
                    }
                    else if (syms[symIdx].InCat(catIdxMid))
                    {
                        domesticMidTotal += a.value;
                    }
                    else if (syms[symIdx].InCat(catIdxLarge))
                    {
                        domesticLargeTotal += a.value;
                    }
                    else
                    {
                        Console.WriteLine($"Error!  Domestic equity {a.symbol} not small, mid, or large???\n\n");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine($"Error!  Equity-categorized {a.symbol} not either foreign or domestic???\n\n");
                    return;
                }
            }
            double categorizedEquityTotal = foreignSmallTotal + foreignMidTotal + foreignLargeTotal
                + domesticSmallTotal + domesticMidTotal + domesticLargeTotal;
            Console.WriteLine($"equity category total = {categorizedEquityTotal}; equity total = {equityTotal}\n\n");

            List<Segment> segs = new List<Segment> {
                new Segment("Foreign small", foreignSmallTotal/equityTotal),
                //new Segment("Foreign mid", foreignMidTotal/equityTotal),
                new Segment("Foreign large", foreignLargeTotal/equityTotal),
                new Segment("Domestic small", domesticSmallTotal/equityTotal),
                new Segment("Domestic mid", domesticMidTotal/equityTotal),
                new Segment("Domestic large", domesticLargeTotal/equityTotal)
            };
            PieChart pc = PieChart.Create(new PieChartConfig(), CollectionsMarshal.AsSpan(segs));
            Console.WriteLine(pc.ToXml());
            Console.WriteLine("\n\n");
            File.WriteAllText("equity.svg", pc.ToXml());
        }

    
    } // Main

}























