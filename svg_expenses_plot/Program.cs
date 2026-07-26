


using System.Reflection.Metadata;
using System.Xml.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Transactions;



class Program
{
    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Usage: program <transactions-file>");
            Console.Error.WriteLine("Example: dotnet run -- ./may.txt > output.svg");
            return;
        }

        List<Transaction>? transactions = Utils.ExtractTransactionList(File.ReadAllLines(args[0]));
        if (transactions == null)
        {
            Console.WriteLine("transactions==null");
            return;
        }
        /*foreach (Transaction t in transactions)
        {
            Console.WriteLine($"{t.dayNum}, {t.cashFlow}");
        }*/

        //XElement svg = SpendPlot.Build(CollectionsMarshal.AsSpan(transactions));
        SpendPlotConfig cfg = new SpendPlotConfig();
        SpendPlot plot = SpendPlot.Create(cfg,CollectionsMarshal.AsSpan(transactions));

        Console.WriteLine(plot.ToXml());
    }

    
}




