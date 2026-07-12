

using System.Xml.Linq;
using System.Text;
using System.Runtime.InteropServices;


class SpendPlot
{
    // XML crap
    public static XNamespace ns = "http://www.w3.org/2000/svg";

    // Width and height of the outer area
    const int areaWidth = 1400;
    const int areaHeight = 800;
    const string areaFill = "#ffffff";
    const string areaStyle = "stroke-width:5; stroke:#00ff00";

    // Marker parameters
    const int markerRadius = 6;
    const string markerFill = "#ffffff";
    const string markerStyle = "stroke:#000000;stroke-width:4";

    // Trace parameters
    const string traceFill = "none"; // transparent
    const string traceStyle = "stroke-width:3; stroke:#000000";

    // Axes parameters
    const string axesFill = "#ffffff";
    const string axesStyle = "stroke-width:4; stroke:#000000";
    const int axesPad = 50; // Offset from outer bounding rect

    // Axes marker parameters (tick marks)
    const string tickFill = "#ffffff";
    const string tickStyle = "stroke-width:4; stroke:#000000";
    const double tickLen = 10.0;

    // Axis label parameters
    const string axisLabelFill = "#000000";
    const int axisLabelFontSize = 35;

    // Target diagional line parameters
    const string diagStyle = "stroke:#000000;stroke-width:4;stroke-dasharray:30";


    //
    // Public methods
    //

    // Creates a single marker.  Point must have "pixel"/"absolute"-space units (NOT days, $).
    public static XElement CreateMarker(Point pt)
    {
        return new XElement(ns+"circle",
            new XAttribute("r",markerRadius),
            new XAttribute("fill",markerFill),
            new XAttribute("style",markerStyle),
            new XAttribute("cx",pt.x), // TODO:  Can cx && cy be floating point?
            new XAttribute("cy",pt.y));
    }

    // Creates the segmented line that connects all the markers.  Points must have "pixel"/"absolute"-space
    // units (NOT days, $).
    public static XElement CreateMarkerTrace(ReadOnlySpan<Point> pts)
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
            new XAttribute("fill",traceFill),
            new XAttribute("style",traceStyle),
            new XAttribute("points",tracePoints.ToString()));
    }

    // Creates the outer bounding rectangle of the image
    public static XElement CreateArea()
    {
        return new XElement(ns+"rect",
            new XAttribute("width",areaWidth),
            new XAttribute("height",areaHeight),
            new XAttribute("fill",areaFill),
            new XAttribute("style",areaStyle));
    }

    // Creates the axes object.  The width and height are those of the outer area rect.  The "padding"
    // member determines the internal offset of the axes within the rect.
    public static XElement CreateAxes()
    {
        string axesPoints =$"{axesPad},{axesPad} {axesPad},{areaHeight-axesPad} {areaWidth-axesPad},{areaHeight-axesPad}";
        return new XElement(ns+"polyline",
            new XAttribute("fill",axesFill),
            new XAttribute("style",axesStyle),
            new XAttribute("points",axesPoints));
    }

    public static XElement CreateAxisMarkerX(double xpos)
    {
        return new XElement(ns+"line",
            new XAttribute("fill",tickFill),
            new XAttribute("style",tickStyle),
            new XAttribute("x1",xpos),
            new XAttribute("y1",750),
            new XAttribute("x2",xpos),
            new XAttribute("y2",750+tickLen));
    }

    public static XElement CreateAxisMarkerY(double ypos)
    {
        return new XElement(ns+"line",
            new XAttribute("fill",tickFill),
            new XAttribute("style",tickStyle),
            new XAttribute("x1",50),
            new XAttribute("y1",ypos),
            new XAttribute("x2",50-tickLen),
            new XAttribute("y2",ypos));
    }

    public static XElement CreateAxisLabelX()
    {
        double x = 750;
        double y = 800.0-50;
        return new XElement(ns+"text",
            new XAttribute("x",x),
            new XAttribute("y",y),
            new XAttribute("fill", axisLabelFill),
            new XAttribute("font-size", axisLabelFontSize),
            "Day number");
    }

    public static XElement CreateAxisLabelY()
    {
        double x = 50;
        double y = 400.0/2.0+100;
        string transform = $"rotate(-90,{x},{y})";
        return new XElement(ns+"text",
            new XAttribute("x",x),
            new XAttribute("y",y),
            new XAttribute("fill", axisLabelFill),
            new XAttribute("font-size", axisLabelFontSize),
            new XAttribute("transform", transform),
            "Cumulative spent");
    }

    public static XElement CreateTargetDiagional()
    {
        Point start = Plot2Abs(new Point{x=1,y=10});  // Note that y==10 @ x==1
        Point end = Plot2Abs(new Point{x=31,y=31*10});

        return new XElement(ns+"line",
            new XAttribute("x1",start.x),
            new XAttribute("y1",start.y),
            new XAttribute("x2",end.x),
            new XAttribute("y2",end.y),
            new XAttribute("style",diagStyle));
    }

    // Absolute coordinates (units of "px") to plot coordinates (units of days, $)
    public static Point Abs2Plot(Point pt)
    {
        double dollarPerPx = (double)PlotDistanceY()/(double)AbsDistanceY();
        double dayPerPx = (double)PlotDistanceX()/(double)AbsDistanceX();
        return new Point{x=dayPerPx*(pt.x-50), y=-1*dollarPerPx*(pt.y-750)};
    }

    // Plot coordinates (pt.x~days, pt.y~$) to absolute ("pixel") coordinates
    public static Point Plot2Abs(Point pt)
    {
        double pxPerDollar = (double)AbsDistanceY()/(double)PlotDistanceY();
        double pxPerDay = (double)AbsDistanceX()/(double)PlotDistanceX();
        return new Point{x=pxPerDay*pt.x+50, y=-1*pxPerDollar*pt.y+750};
    }

    // The input array of transactions might not have a transaction for each day of the month.  It also
    // might have more than one transaction for a given day.  The PlotData List that comes out has a single
    // entry for each day in the month.  Points are in "pixel"/"abs" space so they can be passed directly into
    // CreateMArker() and CreateMarkerTrace(). 
    // Silently ignores any invalid data (like a point with a dayNum <= 0 or > maxPossibleDayNum).  TODO.
    public static List<Point> TransactionsToPlotData(ReadOnlySpan<Transaction> transactions)
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

    // x-axis distance in "pixel" space
    public static int AbsDistanceX()
    {
        return areaWidth-2*axesPad;
    }

    // y-axis distance in "pixel" space
    public static int AbsDistanceY()
    {
        return areaHeight-2*axesPad;
    }

    // x-axis length in "plot" space; units of days
    public static int PlotDistanceX()
    {
        return 31;
    }

    // y-axis length in "plot" space; units of $
    public static int PlotDistanceY()
    {
        return 31*10+20; // Allow space to represent $20 over the limit
    }
}



























