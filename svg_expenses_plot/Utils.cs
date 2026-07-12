using System.Text;
using System.Text.RegularExpressions;




readonly record struct Transaction
{
    public Transaction(int daynum_, double cashFlow_)
    {
        dayNum = daynum_;
        cashFlow = cashFlow_;
    }
    public readonly int dayNum;
    public readonly double cashFlow;
}

struct Point
{
    public double x;
    public double y;
}


class Utils
{
    // Extracts a list of transactions from the contents of a transaction file (presented as an array of
    // individual lines).
    //
    // C# and its built-in libraries are, unfortunately, a joke, and there is no way to run a regex over a
    // span, obtaining pointers or offsets into the span corresponding to the matches.  You basically have
    // to pass around strings, copying them at every step.  I'd like to just pass in a ReadOnlySpan<char>
    // corresponding to the entire file, manually obtain a view over each line, and regex each such view.
    // There is simply no way to do that, so you get the abomination below.
    public static List<Transaction>? ExtractTransactionList(string[] lines)
    {
        List<Transaction> transactions = new List<Transaction>(31);
        foreach (string ln in lines)
        {
            System.Text.RegularExpressions.Match m = Regex.Match(ln,@"\s*(\d+)[\s]+(-?\d*\.?\d+)");
            if (!m.Success)
            {
                continue;
            }
            int daynum = int.Parse(m.Groups[1].ValueSpan);
            double flow = double.Parse(m.Groups[2].ValueSpan);
            transactions.Add(new(daynum,flow));
        }
        return transactions;
    }
}














