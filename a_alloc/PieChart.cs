

using System.Xml.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;


public struct Segment
{
    public Segment(string label, double frac)
    {
        this.label = label;
        this.frac = frac;
    }

    public string label = "";
    public double frac = 0;
}

class PieChartConfig
{
    //
    // Public data
    //

    // Width and height of the outer area
    public int areaWidth = 1400;
    public int areaHeight = 800; //800
    public string areaFill = "#ffffff";
    public string areaStyle = "stroke-width:5; stroke:#00ff00";

    // Scale factor
    // Determines the radius.  Fraction of 1/2 the plot area
    public double scaleFactor = 0.8;

    // Lines seperating the segments
    public string traceStyle = "stroke-width:3; stroke:#000000";

    // Label parameters
    public string labelFill = "#000000";
    public int labelFontSize = 25;
    public string labelFontFamily = "sans-serif";

    // Color parameters
    public double sat = 0.65;
    public double light = 0.55;

    //
    // Public methods
    //
    public PieChartConfig()
    {
        //...
    }

    public Utils.Rgb GetColor(int i)
    {
        double hue = (i * 137.50776405003785) % 360.0;
        return Utils.HslToRgb(hue, sat, light);
    }


}


class PieChart
{
    private static XNamespace ns = "http://www.w3.org/2000/svg";
    private XElement plot;

    //
    // Public methods
    //
    public static PieChart Create(PieChartConfig cfg, ReadOnlySpan<Segment> data)
    {

        XElement plot = PieChart.Build(cfg, data);
        PieChart pc = new PieChart(plot);
        return pc;
    }

    public string ToXml()
    {
        return plot.ToString();
    }


    //
    // Private methods
    //
    private PieChart(XElement plot)
    {
        this.plot = plot;
    }

    // Builds the xml svg object for the plot
    private static XElement Build(PieChartConfig cfg, ReadOnlySpan<Segment> data)
    {
        XElement svg = new XElement(ns+"svg"); // outer <svg>...</svg>
        svg.Add(PieChart.CreateImageArea(cfg));  // Rectangle demacating the whole image

        // Build the segments ensuring the fraction values are normalized
        /*double sum = 0.0;
        foreach (Segment s in data)
        {
            sum += s.frac;
        }*/
        List<XElement> segs = CreateSegments(cfg,data);
        foreach (XElement seg in segs)
        {
            svg.Add(seg);
        }

        //svg.Add(plotArea);
        return svg;
    }

    // Creates a single segment, correctly translated and ready to be .Add()'ed to the main XElement
    private static List<XElement> CreateSegments(PieChartConfig cfg, ReadOnlySpan<Segment> segs)
    {
        double cX = cfg.areaWidth/2.0;
        double cY = cfg.areaHeight/2.0;
        double r = (cfg.scaleFactor)*((cfg.areaHeight)/2.0);
        List<XElement> result = new List<XElement>();
        string translate = string.Format($"translate({cX},{cY})");

        double lastAngle = 0.0;
        double lastX2 = 0.0;
        double lastY2 = 1.0*r;
        int i=0;
        foreach (Segment seg in segs)
        {
            double angle = 2.0*Math.PI*(seg.frac) + lastAngle;
            double x1 = lastX2;
            double y1 = lastY2;
            double x2 = r*Math.Sin(angle);
            double y2 = r*Math.Cos(angle);
            string d = $"M 0.0 0.0 L {x1} {y1} A {r} {r} 0 0 0 {x2} {y2} Z";
            Utils.Rgb fill = cfg.GetColor(i);

            result.Add(new XElement(ns+"path",
                new XAttribute("d",d),
                new XAttribute("transform",translate),
                new XAttribute("fill",$"#{fill.R:X2}{fill.G:X2}{fill.B:X2}"),
                new XAttribute("style",cfg.traceStyle)));

            lastAngle = angle;
            lastX2 = x2;
            lastY2 = y2;
            ++i;
        }
        return result;
    }

 
    // Creates the outer bounding rectangle of the image
    // TODO:  This should be calculated from the dimensions of what it contains?
    private static XElement CreateImageArea(PieChartConfig cfg)
    {
        return new XElement(ns+"rect",
            new XAttribute("width",cfg.areaWidth),
            new XAttribute("height",cfg.areaHeight),
            new XAttribute("fill",cfg.areaFill),
            new XAttribute("style",cfg.areaStyle));
    }
    
}




