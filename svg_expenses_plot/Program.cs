


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

        XElement svg = new XElement(SpendPlot.ns+"svg");
        svg.Add(SpendPlot.CreateArea());

        // Axis with tick marks and labels
        svg.Add(SpendPlot.CreateAxes());
        for (int i=1; i<31; ++i)
        {
            Point dummy = new Point {x=i,y=0};
            svg.Add(SpendPlot.CreateAxisMarkerX(SpendPlot.Plot2Abs(dummy).x));
        }
        for (int i=50; i<=310; i+=50)
        {
            Point dummy = new Point {x=0,y=i};
            svg.Add(SpendPlot.CreateAxisMarkerY(SpendPlot.Plot2Abs(dummy).y));
        }
        svg.Add(SpendPlot.CreateAxisLabelX());
        svg.Add(SpendPlot.CreateAxisLabelY());


        svg.Add(SpendPlot.CreateTargetDiagional());

        //List<Point> plotData = TransactionsToPlotData(SampleData.transactions);
        List<Point> plotData = SpendPlot.TransactionsToPlotData(CollectionsMarshal.AsSpan(transactions));
        foreach (Point pt in plotData)
        {
            svg.Add(SpendPlot.CreateMarker(pt));
        }
        svg.Add(SpendPlot.CreateMarkerTrace(CollectionsMarshal.AsSpan(plotData)));

        Console.WriteLine(svg.ToString());
    }

    
}




