




using a_alloc.Tests;
using Xunit.Sdk;

public class EtfcImporter
{
    


    public static List<Utils.Asset>? Import(string[] lines)
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
                    ReadOnlySpan<char> temp = spl.Current();
                    value = double.Parse(Utils.TrimWhitespace(temp));
                    break;
                }

                ++j;
                spl.GoNext();
            }

            if (symbol.Length==0 || value==double.NaN)
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

        // Validate that the value of all the assets sums to what the table reported as the TOTAL
        // TODO:  Could do all of this while extracting from the table.
        foreach (Utils.Asset a in result)
        {
            totalVal -= a.value;
        }
        if (totalVal > 0.05 || totalVal < -0.05)
        {
            return null; // error
        }

        return result;
    }

}



















