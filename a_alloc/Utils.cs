


using System.ComponentModel;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;

static public class Utils
{
    // When Finished() is true, Current() will return an empty span
    public ref struct Splitter
    {
        public Splitter(char delim, ReadOnlySpan<char> payload)
        {
            this.delim = delim;
            this.payload = payload;

            while (end != payload.Length && payload[end] != delim)
            {
                ++end;
            }
        }

        public bool Finished()
        {
            return (beg == end) && (end == payload.Length);
        }

        public bool GoNext()
        {
            if (Finished())
            {
                return false;
            }

            if (end == payload.Length)
            {
                beg = end;
                return false;  // All done
            }

            // beg is on the first char of the previous group, end is on the the delim
            ++end;
            beg = end;
            if (beg == payload.Length)
            {
                return false; // The payload ended with a delim
            }

            // Seek end to the first delim
            while (end != payload.Length && payload[end] != delim)
            {
                ++end;
            }
            return true;
        }

        public ReadOnlySpan<char> Current()
        {
            return payload.Slice(beg,end-beg);
        }

        private readonly char delim;
        private readonly ReadOnlySpan<char> payload;
        private int beg = 0;
        private int end = 0;
    }

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
        public Symbol(string symbol, List<int> categories)
        {
            this.symbol = symbol;
            this.categories = categories;
        }
        public readonly string symbol;
        public readonly List<int> categories;
    }

    // TODO:  The regex in use here is constraining symbols and category names in ways that
    // ReadConfig() does not.
    public static List<Symbol>? ReadSymbols(string[] lines, in List<string> categories)
    {
        List<Symbol> result = new List<Symbol>();
        var rx = new System.Text.RegularExpressions.Regex(@"\(([a-zA-Z0-9\s]+)\);([,_\sa-zA-Z0-9]+)");
        bool inSymbolList = false;
        foreach (string ln in lines)
        {
            ReadOnlySpan<char> currLn = TrimWhitespace(ln);
            if (currLn.Length == 0 || currLn[0] == '#')
            {
                continue;
            }

            if (!inSymbolList && IsEq(currLn,"[symbols]"))
            {
                inSymbolList = true;
                continue;
            }

            if (!inSymbolList)
            {
                continue;
            }

            // In the symbol list now.  currLn is not a comment and is not empty
            Match? m = rx.Match(currLn.ToString());  // I love how you have to materialize a string
            if (m == null || m.Groups.Count != 3)
            {
                return null; //error
            }

            ReadOnlySpan<char> symbol = m.Groups[1].Value;
            ReadOnlySpan<char> categorySet = m.Groups[2].Value;  // ,-seperated list

            // Now process the list of categories
            List<int> cats = new List<int>();
            Splitter spl = new Splitter(',',categorySet);
            while (!spl.Finished())
            {
                ReadOnlySpan<char> currCat = TrimWhitespace(spl.Current());
                int i = categories.IndexOf(currCat.ToString()); // TODO:  FindIndex
                if (i == -1)
                {
                    return null; // error
                }
                cats.Add(i);
                spl.GoNext();
            }

            Symbol symb = new Symbol(symbol.ToString(), cats);
            result.Add(symb);
        }

        return result;
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


    // To viloate an "exclusive" rule is to contain two (or more) members that belong to the rule.  The two
    // indices here are two members of a category set that belong to the rule.
    // Only one of idxa, idxb being == -1 is probably some sort of error.
    public ref struct RuleViolationExclusive
    {
        // A non-violation (IsEmpty()==true))
        public RuleViolationExclusive()
        {
        }

        public RuleViolationExclusive(int a, int b)
        {
            this.idxa = a;
            this.idxb = b;
        }

        public bool IsEmpty()
        {
            return (idxa == -1) || (idxb == -1);
        }

        public readonly int idxa = -1;
        public readonly int idxb = -1;
    }

    // The category set violates the exclusive rule if more than one entry in the category set is
    // in the rule
    public static RuleViolationExclusive ViloatesExclusiveRule(ReadOnlySpan<int> categorySet, ReadOnlySpan<int> ruleExclusive)
    {
        int firstCatInRule = -1;
        foreach (int catInCatSet in categorySet)
        {
            if (!ruleExclusive.Contains(catInCatSet))
            {
                continue;
            }

            if (firstCatInRule == -1)
            {
                firstCatInRule = catInCatSet;
                continue;
            }

            // catInCatSet is in the rule but firstCatInRule, a member of categorySet, was already 
            // found to be in the rule
            return new RuleViolationExclusive(firstCatInRule, catInCatSet);
        }

        return new RuleViolationExclusive();
    }

    // The category set violates the exhaustive rule if none of the entries in the category set is
    // in the rule
    public static bool ViloatesExhaustiveRule(ReadOnlySpan<int> categorySet, ReadOnlySpan<int> ruleExhaustive)
    {
        foreach(int catInCatSet in categorySet)
        {
            if (ruleExhaustive.Contains(catInCatSet))
            {
                return false; // No violation
            }
        }
        return true; // Violation
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

    public static string DebugPrintAsCsv(List<int> l)
    {
        StringBuilder sb = new StringBuilder();
        foreach (int i in l)
        {
            sb.AppendFormat($"{i}, ");
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




















