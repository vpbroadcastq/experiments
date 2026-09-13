




using a_alloc.Tests;
using Xunit.Sdk;

public class TiaaImporter
{
    public static List<Utils.Asset>? Import(string[] lines)
    {
        List<Utils.Asset> result = new List<Utils.Asset>();
        
        bool foundTable = false;
        int colNumSymbDesc = -1;
        int colNumTotVal = -1;
        double totalValReported = double.NaN; // Total value of all assets reported on the "All Investments" line
        foreach (string currLn in lines)
        {
            if (!foundTable)
            {
                // Seek to the table.  The header looks something like:
                // Your Investments,Total Value,Last Price,Quantity,Change since last close ($)*,Change since last close (%)*,Cost basis ‡,
                // Identify it by looking for the fields "Your Investments", "Total Value"
                // The symbol and description are combined in the "Your Investments" column, ex, "CREF Global Equities R3 (QCGLIX)"
                Utils.Splitter splh = new Utils.Splitter(',', currLn); // "split header"
                bool foundSymbDesc = false;  // "Your investments"
                bool foundTotVal = false;
                int i = 0;
                while (!splh.Finished())
                {
                    if (!foundSymbDesc && Utils.IsEq("Your Investments", Utils.TrimWhitespace(splh.Current())))
                    {
                        colNumSymbDesc = i;
                        foundSymbDesc = true;
                    }
                    else if (!foundTotVal && Utils.IsEq("Total Value", Utils.TrimWhitespace(splh.Current())))
                    {
                        colNumTotVal = i;
                        foundTotVal = true;
                    }
                    ++i;
                    splh.GoNext();
                }
                if (!foundSymbDesc || !foundTotVal)
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

            // Detect and handle the very last line
            if (currLn.StartsWith("All Investments"))
            {
                // This indicates the end of the table
                Utils.Splitter splLl = new Utils.Splitter(',', currLn);  // "split last line"
                splLl.GoNext();
                if (splLl.Finished())
                {
                    return null; // error
                }
                ReadOnlySpan<char> totValStr = Utils.TrimWhitespace(splLl.Current());
                if (totValStr.Length<=1 || !totValStr.StartsWith("$"))
                {
                    return null; // error
                }
                totalValReported = double.Parse(totValStr[1..]); // Chop off the leading $

                break; // All done
            }

            // On a data line of the table (not the last line).  The line is one of three possibilities
            // 1) The name of an account
            // 2) Tot total balance of the account
            // 3) A line with a symbol, value, price, etc
            // 1 and 2 can be detectd by counting the number of commas.  These lines end in commas and have
            // only a single comma.  In contrast, a "data line" will have several commas, seperating all the fields
            // corresponding to the header row.
            if (!Utils.ContainsMoreThanN(currLn,',',1))
            {
                continue; // On either an account-name line or a "Total balance:" line
            }

            // On a data line (case 3 above)
            string symbolAndDescription = "";
            double value = double.NaN;
            Utils.Splitter spl = new Utils.Splitter(',', currLn);
            int j = 0;
            while (!spl.Finished())
            {
                if (j==colNumSymbDesc)
                {
                    symbolAndDescription = Utils.TrimWhitespace(spl.Current()).ToString();
                }
                else if (j==colNumTotVal)
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
            if (symbolAndDescription.Length==0 || value==double.NaN)
            {
                return null; //error
            }

            ReadOnlySpan<char> symbol = ExtractSymbolFromSymbolAndDesciption(symbolAndDescription);
            if (symbol.Length>0)
            {
                result.Add(new Utils.Asset(symbol.ToString(),value));
            }
            else
            {
                result.Add(new Utils.Asset(symbolAndDescription,value));
            }
        }

        if (!foundTable)
        {
            return null;
        }

        // Because the table is split into different accounts, and because different accounts can hold the
        // same asset, the result array might contian multiple entries for the same symbol.  Dedup the List.
        result.Sort((Utils.Asset lhs, Utils.Asset rhs) =>
        {
            return string.CompareOrdinal(lhs.symbol,rhs.symbol);
        });

        for (int i=1; i<result.Count; ++i)
        {
            if (string.CompareOrdinal(result[i-1].symbol, result[i].symbol)!=0)
            {
                continue;
            }
            // result[i-1] and result[i] have the same symbol
            Utils.Asset newEntry = new Utils.Asset(result[i-1].symbol, result[i-1].value + result[i].value);
            result[i-1] = newEntry;
            result.RemoveAt(i);
            --i;
        }

        return result;
    }

    // The "symbol and description" field of a data line looks something like
    // CREF Inflation-Linked Bond R3 (QCILIX)
    // This helper extracts the ()'d symbol if it's present.  Returns an empty span if it isn't.
    // TODO:  Possibly should require the last char of the input to be ) on the thinking that the
    // symbol is proably never burried in the middle of the string.  It's probably always at the very
    // end.
    private static ReadOnlySpan<char> ExtractSymbolFromSymbolAndDesciption(ReadOnlySpan<char> s)
    {
        int beg = -1;
        int end = -1;
        for (int i=s.Length-1; i>0; --i)
        {
            if (beg==-1 && end==-1 && s[i]==')')
            {
                end = i;
                continue;
            }

            if (beg==-1 && end>-1 && s[i]=='(')
            {
                beg = i;
                break;
            }
        }

        if (beg==-1 || end==-1)
        {
            return ReadOnlySpan<char>.Empty;
        }

        // Found the ( and the ), and beg<end
        ++beg;
        if (!(beg<end))
        {
            return ReadOnlySpan<char>.Empty;
        }
        return s[beg..end];
    }

}



















