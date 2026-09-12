




using a_alloc.Tests;
using Xunit.Sdk;

public class FidelityImporter
{
    public static List<Utils.Asset>? Import(string[] lines)
    {
        List<Utils.Asset> result = new List<Utils.Asset>();
        
        bool foundTable = false;
        int colNumSymb = -1;
        int colNumDesc = -1;
        int colNumCv = -1;
        foreach (string currLn in lines)
        {
            if (!foundTable)
            {
                // Seek to the table.  The header looks something like:
                // Account number,Account name,Symbol,Description,CUSIP,Current value,Total gain/loss %,Today's gain/loss %,Security type,Security subtype
                // Identify it by looking for the fields "Symbol", "Description" and "Current value"
                Utils.Splitter splh = new Utils.Splitter(',', currLn);
                bool foundSymb = false;
                bool foundDesc = false; // TODO could use the colNum* vars right?  These are not needed
                bool foundCv = false;
                int i = 0;
                while (!splh.Finished())
                {
                    if (!foundSymb && Utils.IsEq("Symbol", Utils.TrimWhitespace(splh.Current())))
                    {
                        colNumSymb = i;
                        foundSymb = true;
                    }
                    else if (!foundDesc && Utils.IsEq("Description", Utils.TrimWhitespace(splh.Current())))
                    {
                        colNumDesc = i;
                        foundDesc = true;
                    }
                    else if (!foundCv && Utils.IsEq("Current value", Utils.TrimWhitespace(splh.Current())))
                    {
                        colNumCv = i;
                        foundCv = true;
                    }
                    ++i;
                    splh.GoNext();
                }
                if (!foundSymb || !foundDesc || !foundCv)
                {
                    continue;
                }

                foundTable = true;
                continue;
            }

            // Found the table.  Expect to be on a data line, or on the first line after the last
            // line of the data table
            // foundTable == true
            // colNumBlah != -1
            if (currLn.Length == 0)
            {
                break; // On the first line after the last data line of the table
            }

            // On a data line of the table.  Get the symbol and value 
            string symbol = "";
            string desc = "";
            double value = double.NaN;
            Utils.Splitter spl = new Utils.Splitter(',', currLn);
            int j = 0;
            while (!spl.Finished())
            {
                if (j==colNumSymb)
                {
                    symbol = Utils.TrimWhitespace(spl.Current()).ToString();
                }
                else if (j==colNumDesc)
                {
                    desc = Utils.TrimWhitespace(spl.Current()).ToString();
                }
                else if (j==colNumCv)
                {
                    // The field starts w/a $, ex "$123.45"
                    ReadOnlySpan<char> fieldStr = Utils.TrimWhitespace(spl.Current());
                    if (!fieldStr.StartsWith('$'))
                    {
                        return null; // error
                    }
                    value = double.Parse(fieldStr[1..]);
                }

                // TODO:  Break early once j >= max(colNumBlah);
                ++j;
                spl.GoNext();
            }

            // There won't always be a symbol.  If symb is empty, use the description instead.
            if ((symbol.Length==0 && desc.Length == 0) || (value==double.NaN))
            {
                return null; // error
            }

            if (symbol.Length > 0)
            {
                result.Add(new Utils.Asset(symbol,value));
            }
            else
            {
                result.Add(new Utils.Asset(desc,value));
            }
        }

        if (!foundTable)
        {
            return null;
        }

        return result;
    }

}



















