

using System.Xml.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;


class SpendPlotConfig
{
    //
    // Public data
    //

    // Width and height of the outer area
    public int areaWidth = 1400;
    public int areaHeight = 800; //800
    public int xyPad = 100;
    public string areaFill = "#ffffff";
    public string areaStyle = "stroke-width:5; stroke:#00ff00";

    // Marker parameters
    public int markerRadius = 6;
    public string markerFill = "#ffffff";
    public string markerStyle = "stroke:#000000;stroke-width:4";

    // Trace parameters
    public string traceFill = "none"; // transparent
    public string traceStyle = "stroke-width:3; stroke:#000000";

    // Axes parameters
    public string axesFill = "#ffffff";
    public string axesStyle = "stroke-width:4; stroke:#000000";

    // Axes marker parameters (tick marks)
    public string tickFill = "#ffffff";
    public string tickStyle = "stroke-width:4; stroke:#000000";
    public double tickLen = 10.0;

    // Axis label parameters
    public string axisLabelFill = "#000000";
    public int axisLabelFontSize = 30;
    public string axisLabelFontFamily = "sans-serif";

    // Tick label parameters
    public string tickLabelFill = "#000000";
    public int tickLabelFontSize = 20;
    public string tickLabelFontFamily = "sans-serif";

    // Target diagional line parameters
    public string diagStyle = "stroke:#000000;stroke-width:4;stroke-dasharray:30";

    //
    // Public methods
    //
    public SpendPlotConfig()
    {
        //...
    }

    // x-axis distance in "pixel" space
    public int AbsDistanceX()
    {
        return areaWidth-2*xyPad;
    }

    // y-axis distance in "pixel" space
    public int AbsDistanceY()
    {
        return areaHeight-2*xyPad;
    }

    // x-axis length in "plot" space; units of days
    public int PlotDistanceX()
    {
        return 31;
    }

    // y-axis length in "plot" space; units of $
    public int PlotDistanceY()
    {
        return 31*10+20; // Allow space to represent $20 over the limit
    }

    // Absolute coordinates (units of "px") to plot coordinates (units of days, $)
    public Point Abs2Plot(Point pt)
    {
        double dollarPerPx = (double)PlotDistanceY()/(double)AbsDistanceY();
        double dayPerPx = (double)PlotDistanceX()/(double)AbsDistanceX();
        return new Point{x=dayPerPx*(pt.x), y=-1*dollarPerPx*(pt.y-AbsDistanceY())};
    }

    // Plot coordinates (pt.x~days, pt.y~$) to absolute ("pixel") coordinates
    public Point Plot2Abs(Point pt)
    {
        double pxPerDollar = (double)AbsDistanceY()/(double)PlotDistanceY();
        double pxPerDay = (double)AbsDistanceX()/(double)PlotDistanceX();
        return new Point{x=pxPerDay*pt.x, y=-1*pxPerDollar*pt.y+AbsDistanceY()};
    }

    // The input array of transactions might not have a transaction for each day of the month.  It also
    // might have more than one transaction for a given day.  The PlotData List that comes out has a single
    // entry for each day in the month.  Points are in "pixel"/"abs" space so they can be passed directly into
    // CreateMArker() and CreateMarkerTrace(). 
    // Silently ignores any invalid data (like a point with a dayNum <= 0 or > maxPossibleDayNum).  TODO.
    public List<Point> TransactionsToPlotData(ReadOnlySpan<Transaction> transactions)
    {
        const int maxPossibleDayNum = 31;
        List<Point> plotData = new List<Point>();
        for (int i=1; i<=maxPossibleDayNum; ++i)
        {
            plotData.Add(new Point {x=i,y=0});
        }
        Span<Point> spanPlotData = CollectionsMarshal.AsSpan(plotData);  // No other way to ref into a List

        int maxDayNum = 0;
        foreach (Transaction curr in transactions)
        {
            if (curr.dayNum <= 0 || curr.dayNum > maxPossibleDayNum)
            {
                continue;
            }
            maxDayNum = int.Max(maxDayNum,curr.dayNum);
            spanPlotData[curr.dayNum-1].y += curr.cashFlow;
        }
        if (maxDayNum != maxPossibleDayNum)
        {
            // Truncate the list.  The idx of maxDayNum is maxDayNum-1, but i want to keep that one, so remove
            // [maxDayNum,]
            plotData.RemoveRange(maxDayNum,plotData.Count-maxDayNum);
            spanPlotData = CollectionsMarshal.AsSpan(plotData);
        }

        // Integrate.  This can't be done as part of the first loop because the input may not be sorted by dayNum
        double cumSum = 0.0;
        foreach (ref Point pt in spanPlotData)
        {
            cumSum += pt.y;
            pt.y = cumSum;
        }

        // Transform from "plot" coordinates (day,$) to "pixel/absolute" coordinates
        foreach (ref Point pt in spanPlotData)
        {
            pt = Plot2Abs(pt);
        }

        return plotData;
    }
}

class SpendPlot
{
    private static XNamespace ns = "http://www.w3.org/2000/svg";
    private XElement plot;

    //
    // Public methods
    //
    public static SpendPlot Create(SpendPlotConfig cfg, ReadOnlySpan<Transaction> transactions)
    {

        XElement plot = SpendPlot.Build(cfg, transactions);
        SpendPlot sp = new SpendPlot(plot);
        return sp;
    }

    public string ToXml()
    {
        return plot.ToString();
    }


    //
    // Private methods
    //
    private SpendPlot(XElement plot)
    {
        this.plot = plot;
    }

    private static XElement Build(SpendPlotConfig cfg, ReadOnlySpan<Transaction> transactions)
    {
        XElement svg = new XElement(ns+"svg"); // outer <svg>...</svg>
        svg.Add(SpendPlot.CreateImageArea(cfg));  // Rectangle demacating the whole image
        XElement plotArea = SpendPlot.CreatePlotArea(cfg);  // <g> translated relative to the padding

        // Axis with tick marks and labels
        plotArea.Add(SpendPlot.CreateAxes(cfg));
        for (int i=1; i<=31; ++i) // TODO:  31 is hardcoded
        {
            Point dummy = new Point {x=i,y=0};
            plotArea.Add(SpendPlot.CreateAxisMarkerX(cfg, cfg.Plot2Abs(dummy).x));
            plotArea.Add(SpendPlot.CreateTickLabelX(cfg, cfg.Plot2Abs(dummy).x, i));
        }
        for (int i=50; i<=310; i+=50) // TODO:  310 is hardcoded
        {
            Point dummy = new Point {x=0,y=i};
            plotArea.Add(SpendPlot.CreateAxisMarkerY(cfg, cfg.Plot2Abs(dummy).y));
            plotArea.Add(SpendPlot.CreateTickLabelY(cfg, cfg.Plot2Abs(dummy).y, i));
        }
        plotArea.Add(SpendPlot.CreateAxisLabelX(cfg));
        plotArea.Add(SpendPlot.CreateAxisLabelY(cfg));


        plotArea.Add(SpendPlot.CreateTargetDiagional(cfg));

        //List<Point> plotData = TransactionsToPlotData(SampleData.transactions);
        List<Point> plotData = cfg.TransactionsToPlotData(transactions);
        foreach (Point pt in plotData)
        {
            plotArea.Add(SpendPlot.CreateMarker(cfg, pt));
        }
        plotArea.Add(SpendPlot.CreateMarkerTrace(cfg, CollectionsMarshal.AsSpan(plotData)));

        svg.Add(plotArea);
        return svg;
    }

    // Creates a single marker.  Point must have "pixel"/"absolute"-space units (NOT days, $).
    private static XElement CreateMarker(SpendPlotConfig cfg, Point pt)
    {
        return new XElement(ns+"circle",
            new XAttribute("r",cfg.markerRadius),
            new XAttribute("fill",cfg.markerFill),
            new XAttribute("style",cfg.markerStyle),
            new XAttribute("cx",pt.x), // TODO:  Can cx && cy be floating point?
            new XAttribute("cy",pt.y));
    }

    // Creates the segmented line that connects all the markers.  Points must have "pixel"/"absolute"-space
    // units (NOT days, $).
    private static XElement CreateMarkerTrace(SpendPlotConfig cfg, ReadOnlySpan<Point> pts)
    {
        StringBuilder tracePoints= new StringBuilder();
        foreach (Point curr in pts)
        {
            tracePoints.Append($"{curr.x},{curr.y} ");
        }
        if (tracePoints.Length > 0)
        {
            tracePoints.Remove(tracePoints.Length-1,1); // Remove the trailing space
        }

        return new XElement(ns+"polyline",
            new XAttribute("fill",cfg.traceFill),
            new XAttribute("style",cfg.traceStyle),
            new XAttribute("points",tracePoints.ToString()));
    }

    // Creates the outer bounding rectangle of the image
    // TODO:  This should be calculated from the dimensions of what it contains?
    private static XElement CreateImageArea(SpendPlotConfig cfg)
    {
        return new XElement(ns+"rect",
            new XAttribute("width",cfg.areaWidth),
            new XAttribute("height",cfg.areaHeight),
            new XAttribute("fill",cfg.areaFill),
            new XAttribute("style",cfg.areaStyle));
    }

    // <g> translated by the padding.  Tick & axis labels are outside; plot elements are inside.
    // This allows everything added subsequently to be positioned relative to this box, ignoring the padding
    private static XElement CreatePlotArea(SpendPlotConfig cfg)
    {
        string translate = $"translate({cfg.xyPad} {cfg.xyPad})";
        return new XElement(ns+"g",
            new XAttribute("transform",translate));
    }

    // Creates the axes object.  The width and height are those of the outer area rect.  The "padding"
    // member determines the internal offset of the axes within the rect.
    private static XElement CreateAxes(SpendPlotConfig cfg)
    {
        //string axesPoints =$"{axesPad},{axesPad} {axesPad},{areaHeight-axesPad} {areaWidth-axesPad},{areaHeight-axesPad}";
        string axesPoints =$"{0},{0} {0},{cfg.AbsDistanceY()} {cfg.AbsDistanceX()},{cfg.AbsDistanceY()}";
        return new XElement(ns+"polyline",
            new XAttribute("fill",cfg.axesFill),
            new XAttribute("style",cfg.axesStyle),
            new XAttribute("points",axesPoints));
    }

    private static XElement CreateAxisMarkerX(SpendPlotConfig cfg, double xpos)
    {
        return new XElement(ns+"line",
            new XAttribute("fill",cfg.tickFill),
            new XAttribute("style",cfg.tickStyle),
            new XAttribute("x1",xpos),
            new XAttribute("y1",cfg.AbsDistanceY()),
            new XAttribute("x2",xpos),
            new XAttribute("y2",cfg.AbsDistanceY()+cfg.tickLen));
    }

    private static XElement CreateAxisMarkerY(SpendPlotConfig cfg, double ypos)
    {
        return new XElement(ns+"line",
            new XAttribute("fill",cfg.tickFill),
            new XAttribute("style",cfg.tickStyle),
            new XAttribute("x1",0),
            new XAttribute("y1",ypos),
            new XAttribute("x2",-1*cfg.tickLen),
            new XAttribute("y2",ypos));
    }

    // The fact that val is an int isn't very generic.  Could pass in an object and call ToString?
    private static XElement CreateTickLabelX(SpendPlotConfig cfg, double xpos, int val)
    {
        double y = cfg.AbsDistanceY() + cfg.axisLabelFontSize;
        return new XElement(ns+"text",
            new XAttribute("x",xpos),
            new XAttribute("y",y),
            new XAttribute("fill", cfg.tickLabelFill),
            new XAttribute("font-size", cfg.tickLabelFontSize),
            new XAttribute("font-family", cfg.tickLabelFontFamily),
            new XAttribute("text-anchor", "top"),
            new XAttribute("dominant-baseline", "top"),
            val);
    }

    private static XElement CreateAxisLabelX(SpendPlotConfig cfg)
    {
        double x = cfg.AbsDistanceX()/2.0;
        double y = cfg.AbsDistanceY() + cfg.axisLabelFontSize + cfg.tickLabelFontSize;
        return new XElement(ns+"text",
            new XAttribute("x",x),
            new XAttribute("y",y),
            new XAttribute("fill", cfg.axisLabelFill),
            new XAttribute("font-size", cfg.axisLabelFontSize),
            new XAttribute("font-family", cfg.axisLabelFontFamily),
            new XAttribute("text-anchor", "middle"),
            new XAttribute("dominant-baseline", "middle"),
            "Day number");
    }

    // The fact that val is an int isn't very generic.  Could pass in an object and call ToString?
    private static XElement CreateTickLabelY(SpendPlotConfig cfg, double ypos, int val)
    {
        double x = 0.0 - cfg.axisLabelFontSize;
        return new XElement(ns+"text",
            new XAttribute("x",x),
            new XAttribute("y",ypos),
            new XAttribute("fill", cfg.tickLabelFill),
            new XAttribute("font-size", cfg.tickLabelFontSize),
            new XAttribute("font-family", cfg.tickLabelFontFamily),
            new XAttribute("text-anchor", "middle"),
            new XAttribute("dominant-baseline", "middle"),
            val);
    }

    private static XElement CreateAxisLabelY(SpendPlotConfig cfg)
    {
        // TODO:  the *2 is an arbitrary fudge factor
        double x = 0.0 - cfg.axisLabelFontSize - cfg.tickLabelFontSize*2;
        double y = cfg.AbsDistanceY()/2.0;
        string transform = $"rotate(-90,{x},{y})";
        return new XElement(ns+"text",
            new XAttribute("x",x),
            new XAttribute("y",y),
            new XAttribute("fill", cfg.axisLabelFill),
            new XAttribute("font-size", cfg.axisLabelFontSize),
            new XAttribute("font-family", cfg.axisLabelFontFamily),
            new XAttribute("transform", transform),
            new XAttribute("text-anchor", "middle"),
            new XAttribute("dominant-baseline", "middle"),
            "Cumulative spent");
    }

    private static XElement CreateTargetDiagional(SpendPlotConfig cfg)
    {
        Point start = cfg.Plot2Abs(new Point{x=1,y=10});  // Note that y==10 @ x==1
        Point end = cfg.Plot2Abs(new Point{x=31,y=31*10});

        return new XElement(ns+"line",
            new XAttribute("x1",start.x),
            new XAttribute("y1",start.y),
            new XAttribute("x2",end.x),
            new XAttribute("y2",end.y),
            new XAttribute("style",cfg.diagStyle));
    }

    

    

    
}



