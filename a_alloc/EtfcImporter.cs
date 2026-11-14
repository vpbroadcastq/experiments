



using System.Text.RegularExpressions;

public class EtfcImporter : Importer
{
    public bool IsValid(string[] lines)
    {
        bool foundAccountSummary = false;
        bool foundViewSummary = false;
        bool foundGeneratedAt = false;
        foreach (string ln in lines)
        {
            if (!foundAccountSummary && ln.Contains("Account Summary", StringComparison.Ordinal))
            {
                foundAccountSummary = true;
                continue;
            }
            if (!foundViewSummary && ln.Contains("View Summary", StringComparison.Ordinal))
            {
                foundViewSummary = true;
                continue;
            }
            if (!foundGeneratedAt && ln.Contains("Generated at", StringComparison.Ordinal))
            {
                foundGeneratedAt = true;
                continue;
            }
        }
        return foundAccountSummary && foundViewSummary && foundGeneratedAt;
    }

    public DateTime? Date(string[] lines)
    {
        int idxGeneratedAt = -1;
        for (int i=0; i<lines.Count(); ++i)
        {
            if (lines[i].Contains("Generated at", StringComparison.Ordinal))
            {
                idxGeneratedAt = i;
                break;
            }
        }
        if (idxGeneratedAt == -1)
        {
            return null;
        }

        Regex rx = new Regex(@"([a-zA-Z]{3,})\s(\d+)\s(\d+)\s");
        Match? m = rx.Match(lines[idxGeneratedAt]);
        if (m==null || !m.Success || m.Groups.Count!=4)
        {
            return null;
        }
        int? month = Utils.ToMonthNumber(m.Groups[1].ValueSpan);
        if (month==null)
        {
            return null;
        }
        int day = int.Parse(m.Groups[2].ValueSpan);
        int year = int.Parse(m.Groups[3].ValueSpan);

        return new DateTime(year, month.Value, day);
    }


    public List<Utils.Asset>? Import(string[] lines)
    {
        List<Utils.Asset> result = new List<Utils.Asset>();
        
        bool foundTable = false;
        int ColNumValue = -1; // col # of the "Value $" col; first col is 0
        double totalVal = double.NaN; // Total value of all assets reported in the table
        foreach (string currLn in lines)
        {
            // Seek to the table.  The header looks something like:
            // Symbol,Last Price $,Change $,Change %,Quantity,Price Paid $,Day's Gain $,Total Gain $,Total Gain %,Value $
            // Identify it by looking for the fields "Symbol" and "Value $"
            if (!foundTable && (!currLn.StartsWith("Symbol") || !currLn.Contains("Value $")))
            {
                continue;
            }
            if (!foundTable)
            {
                // On the header line.  Find the "Value $" column
                foundTable = true;
                // Figure out which column is the "Value $" column
                Utils.Splitter hspl = new Utils.Splitter(',', currLn);
                int i = 0;
                while (!hspl.Finished())
                {
                    if (Utils.IsEq("Value $", Utils.TrimWhitespace(hspl.Current())))
                    {
                        ColNumValue = i;
                        break;
                    }
                    ++i;
                    hspl.GoNext();
                }
                if (ColNumValue == -1)
                {
                    // The header line does not appear to contain a "Value $" column
                    return null; // error
                }

                continue;  // On the header line; skip it
            }
            
            // currLn is either inside or one past the table of assets, and ColNumValue has been assigned
            // (foundTable && ColNumValue != -1)

            // Get the symbol and value 
            string symbol = "";
            double value = double.NaN;
            Utils.Splitter spl = new Utils.Splitter(',', currLn);
            int j = 0;
            while (!spl.Finished())
            {
                if (j==0)
                {
                    // The first column is the symbol
                    symbol = Utils.TrimWhitespace(spl.Current()).ToString();
                }
                
                if (j == ColNumValue)
                {
                    if (!double.TryParse(Utils.TrimWhitespace(spl.Current()), out value))
                    {
                        return null; // error
                    }
                    break;
                }

                ++j;
                spl.GoNext();
            }

            if (symbol.Length==0 || double.IsNaN(value))
            {
                return null; // error
            }
            if (symbol=="TOTAL")
            {
                totalVal = value;
                break; // The TOTAL row is always the last row in the table
            }

            result.Add(new Utils.Asset(symbol,value));
        }

        if (!foundTable)
        {
            return null; // error
        }

        // Validate that the value of all the assets sums to what the table reported as the TOTAL
        // TODO:  Could do all of this while extracting from the table.
        foreach (Utils.Asset a in result)
        {
            totalVal -= a.value;
        }
        if (Math.Abs(totalVal) > 0.005)
        {
            return null; // error
        }

        return result;
    }

}



















