


using System.ComponentModel;
using System.Text;

static class Utils
{
    public struct ConfigData
    {
        public ConfigData(List<string> categories, List<string> rulesExclusive, List<string> rulesExhaustive)
        {
            this.categories = categories;
            this.rulesExclusive = rulesExclusive;
            this.rulesExhaustive = rulesExhaustive;
        }

        public readonly List<string> categories;
        public readonly List<string> rulesExclusive;
        public readonly List<string> rulesExhaustive;
    }

    // If i was trying to build something good, i wouldn't be using C#
    public struct Symbol
    {
        public readonly string symbol;
        public readonly List<int> categories;
    }

    // TODO:  The regex in use here is constraining symbols and category names in ways that
    // ReadConfig() does not.
    public static Symbol? ReadSymbols(string[] lines)
    {
        string rx = @"\(([a-zA-Z0-9\s]+)\);([,\sa-zA-Z0-9]+)";
        foreach (string ln in lines)
        {
            //...
        }
    }


    // Does the work to process the main config file definining the categories and rules
    // Ensures
    // 1)  All symbols are valid
    // 2)  No duplicate symbols in the set of categories
    // 3)  Rule data is valid (ex, all symbols exist)
    // I am returning the rules as ,-seperated strings just as they are in the config file.  This discards
    // the work done to split the strings and match the entries against the category list, but i am not sure
    // at this point the best way to represent the rules.  TODO.
    // TODO:  Better error reporting
    // TODO:  The rules should be checked to be sure they do not contain duplicate symbols
    public static ConfigData? ReadConfig(string[] lines)
    {
        const string headerCats = "[categories]";
        const string headerExcl = "[exclusive]";
        const string headerExh = "[exhaustive]";

        List<string> categories = new List<string>();
        List<string> rulesExclusive = new List<string>();
        List<string> rulesExhaustive = new List<string>();
        List<string>? currList = null;

        foreach (string ln in lines)
        {
            ReadOnlySpan<char> currln = TrimWhitespace(ln);
            if (currln.Length == 0 || currln[0] == '#')
            {
                continue;
            }

            // Assign the pointer currList to the appropriate list if the present line is a new section header
            if (IsEq(currln,headerCats))
            {
                if (categories.Count > 0)
                {
                    return null; // error
                }
                currList = categories;
                continue;
            }
            else if (IsEq(currln,headerExcl))
            {
                if (rulesExclusive.Count > 0)
                {
                    return null; // error
                }
                currList = rulesExclusive;
                continue;
            }
            else if (IsEq(currln,headerExh))
            {
                if (rulesExhaustive.Count > 0)
                {
                    return null; // error
                }
                currList = rulesExhaustive;
                continue;
            }

            // currln is not a header, is not whitespace, and is not a comment.  We must be inside a section
            // (currList != null) and currln is an entry in that section.
            if (currList == null)
            {
                return null; // error
            }
            if (currList.Contains(currln.ToString()))
            {
                return null; // error
            }
            currList.Add(currln.ToString());
        }

        // Validate that each entry in rulesExclusive is a member of categories
        foreach (string currRule in rulesExclusive)
        {
            ReadOnlySpan<char> ruleRemaining = currRule;
            while (ruleRemaining.Length > 0)
            {
                int end = ruleRemaining.IndexOf(',');
                if (end == -1)
                {
                    end = ruleRemaining.Length;
                }
                ReadOnlySpan<char> ruleEntry = ruleRemaining.Slice(0,end);
                ruleEntry = TrimWhitespace(ruleEntry);
                if (!Contains(ruleEntry, categories))
                {
                    return null; // error
                }
                if (end == ruleRemaining.Length)
                {
                    break;
                }
                ruleRemaining = ruleRemaining.Slice(end+1,ruleRemaining.Length-(end+1));
            }
        }

        // Validate that each entry in rulesExchaustive is a member of categories
        // TODO:  Copy-paste of the block above
        foreach (string currRule in rulesExhaustive)
        {
            ReadOnlySpan<char> ruleRemaining = currRule;
            while (ruleRemaining.Length > 0)
            {
                int end = ruleRemaining.IndexOf(',');
                if (end == -1)
                {
                    end = ruleRemaining.Length;
                }
                ReadOnlySpan<char> ruleEntry = ruleRemaining.Slice(0,end);
                ruleEntry = TrimWhitespace(ruleEntry);
                if (!Contains(ruleEntry, categories))
                {
                    return null; // error
                }
                if (end == ruleRemaining.Length)
                {
                    break;
                }
                ruleRemaining = ruleRemaining.Slice(end+1,ruleRemaining.Length-(end+1));
            }
        }

        ConfigData result = new ConfigData(categories, rulesExclusive, rulesExhaustive);
        return result;
    }




    public static ReadOnlySpan<char> TrimWhitespace(ReadOnlySpan<char> s)
    {
        int beg = 0;
        while (beg < s.Length)
        {
            if (System.Char.IsWhiteSpace(s[beg]))
            {
                ++beg;
            }
            else
            {
                break;
            }
        }

        int end = s.Length - 1;
        while (end >= 0)
        {
            if (System.Char.IsWhiteSpace(s[end]))
            {
                --end;
            }
            else
            {
                break;
            }
        }
        ++end; // go one past the last non-whitespace

        return s.Slice(beg,(end-beg));
    }

    // Avoids converting the needle into a string
    public static bool Contains(ReadOnlySpan<char> n, List<string> h)
    {
        for (int i=0; i<h.Count; ++i)
        {
            if (IsEq(h[i],n))
            {
                return true;
            }
        }
        return false;
    }

    public static string DebugPrint(List<string> ls)
    {
        StringBuilder sb = new StringBuilder();
        int i = 0;
        foreach (string s in ls)
        {
            sb.AppendFormat($"{i}: {s}\n");
            ++i;
        }
        return sb.ToString();
    }

    public static bool IsEq(ReadOnlySpan<char> lhs, ReadOnlySpan<char> rhs)
    {
        if (lhs.Length != rhs.Length)
        {
            return false;
        }

        for (int i=0; i< lhs.Length; ++i)
        {
            if (lhs[i] != rhs[i])
            {
                return false;
            }
        }

        return true;
    }
}




















