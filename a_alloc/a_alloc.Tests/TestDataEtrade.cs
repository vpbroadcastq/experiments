internal static partial class TestData
{
    //
    // E*TRADE - Valid
    //

    // Basic valid E*TRADE file. Every position has a Symbol and Value $,
    // and the position values sum exactly to the TOTAL row.
    public static readonly string validEtradeExampleA = """
Account Summary
Account,Total Assets,Total Unrealized Gain $,Total Unrealized Gain %,Day's Gain Unrealized $,Day's Gain Unrealized %
"Harbor Ridge Brokerage",45838.58,9217.43,25.17,184.26,.40


View Summary - All Positions
Filters applied:
Symbol,Security type(s),Sort by,Sort order,
,All,Symbol,Asc,

Symbol,Last Price $,Change $,Change %,Quantity,Price Paid $,Day's Gain $,Total Gain $,Total Gain %,Value $
QZRA,58.72,0.44,0.76,247.4800,42.1500,108.89,4100.27,39.30,14532.18
QZRB,113.35,-0.62,-0.54,246.0056,89.2200,-152.52,5933.14,27.02,27884.63
CASH,1.00,0.00,0.00,3421.7700,1.0000,0.00,0.00,0.00,3421.77
TOTAL,,,,,,184.26,9217.43,25.17,45838.58

Generated at Aug 20 2026 11:42 AM ET
""";

    public static readonly List<Utils.Asset> resultValidEtradeExampleA =
        new List<Utils.Asset>
        {
            new Utils.Asset("QZRA", 14532.18),
            new Utils.Asset("QZRB", 27884.63),
            new Utils.Asset("CASH", 3421.77)
        };


    // Valid file containing a security whose Symbol field is a long
    // description-like identifier, as may occur with fixed-income holdings.
    public static readonly string validEtradeExampleB = """
Account Summary
Account,Total Assets,Total Unrealized Gain $,Total Unrealized Gain %,Day's Gain Unrealized $,Day's Gain Unrealized %
"Juniper Valley Investment Account",27521.53,3642.91,15.26,-37.18,-.13


View Summary - All Positions
Filters applied:
Symbol,Security type(s),Sort by,Sort order,
,All,Symbol,Asc,

Symbol,Last Price $,Change $,Change %,Quantity,Price Paid $,Day's Gain $,Total Gain $,Total Gain %,Value $
NORTHSTAR GOVERNMENT NOTE 0% 11/19/2027,98.7650,--,--,10.0000,97.8100,.0000,9.55,.9764,987.65
QZMX,44.91,0.17,0.38,497.5651,38.1200,84.59,3378.22,17.80,22345.67
QZTN,76.15,-0.38,-0.50,55.0000,71.5300,-20.90,254.14,6.46,4188.21
TOTAL,,,,,,-37.18,3642.91,15.26,27521.53

Generated at Aug 18 2026 02:07 PM ET
""";

    public static readonly List<Utils.Asset> resultValidEtradeExampleB =
        new List<Utils.Asset>
        {
            new Utils.Asset("NORTHSTAR GOVERNMENT NOTE 0% 11/19/2027", 987.65),
            new Utils.Asset("QZMX", 22345.67),
            new Utils.Asset("QZTN", 4188.21)
        };


    // Valid because the required columns are identified by name rather than
    // by a fixed column index. Irrelevant columns may occur in a different
    // order.
    public static readonly string validEtradeExampleC = """
Account Summary
Account,Total Assets,Total Unrealized Gain $,Total Unrealized Gain %,Day's Gain Unrealized $,Day's Gain Unrealized %
"Copper Lake Securities Account",14681.05,2781.20,23.37,51.74,.35


View Summary - All Positions

Symbol,Quantity,Last Price $,Value $,Change %,Total Gain $,Price Paid $
QZXA,64.0000,80.0063,5120.40,0.41,621.33,70.3000
QZXB,107.0000,83.1785,8900.10,-0.12,1879.20,65.6140
QZXC,15.0000,44.0367,660.55,0.07,280.67,25.3253
CASH,0.0000,1.00,0.00,0.00,0.00,1.0000
TOTAL,,,14681.05,,2781.20,

Generated at Aug 17 2026 09:31 AM ET
""";

    public static readonly List<Utils.Asset> resultValidEtradeExampleC =
        new List<Utils.Asset>
        {
            new Utils.Asset("QZXA", 5120.40),
            new Utils.Asset("QZXB", 8900.10),
            new Utils.Asset("QZXC", 660.55),
            new Utils.Asset("CASH", 0.00)
        };


    // Valid with several ordinary securities and no CASH position.
    public static readonly string validEtradeExampleD = """
Account Summary
Account,Total Assets,Total Unrealized Gain $,Total Unrealized Gain %,Day's Gain Unrealized $,Day's Gain Unrealized %
"Willow Creek Individual Account",26123.44,4891.77,23.04,92.61,.36


View Summary - All Positions

Symbol,Last Price $,Change $,Change %,Quantity,Price Paid $,Day's Gain $,Total Gain $,Total Gain %,Value $
QZLP,125.00,1.21,.98,140.0000,102.3400,169.40,3172.40,22.15,17500.00
QZSV,64.35,-0.18,-.28,126.2393,55.7000,-22.72,1091.45,15.52,8123.45
QZBC,49.999,0.03,.06,10.0000,43.2050,.30,67.92,15.72,499.99
TOTAL,,,,,,92.61,4891.77,23.04,26123.44

Generated at Aug 19 2026 04:16 PM ET
""";

    public static readonly List<Utils.Asset> resultValidEtradeExampleD =
        new List<Utils.Asset>
        {
            new Utils.Asset("QZLP", 17500.00),
            new Utils.Asset("QZSV", 8123.45),
            new Utils.Asset("QZBC", 499.99)
        };


    //
    // E*TRADE - Invalid
    //

    // Invalid because the file contains no positions table.
    public static readonly string invalidEtradeNoPositionsTable = """
Account Summary
Account,Total Assets,Total Unrealized Gain $,Total Unrealized Gain %,Day's Gain Unrealized $,Day's Gain Unrealized %
"Silver Pine Brokerage Account",18942.61,2187.55,12.91,44.10,.23

Generated at Aug 20 2026 10:12 AM ET
""";


    // Invalid because the positions table does not contain the required
    // Symbol column.
    public static readonly string invalidEtradeMissingSymbolColumn = """
View Summary - All Positions

Last Price $,Change $,Change %,Quantity,Price Paid $,Day's Gain $,Total Gain $,Total Gain %,Value $
71.20,0.25,.35,100.0000,62.10,25.00,910.00,14.66,7120.00
44.30,-0.10,-.23,200.0000,39.25,-20.00,1010.00,12.87,8860.00
,,,,,5.00,1920.00,13.68,15980.00
""";


    // Invalid because the positions table does not contain the required
    // Value $ column.
    public static readonly string invalidEtradeMissingValueColumn = """
View Summary - All Positions

Symbol,Last Price $,Change $,Change %,Quantity,Price Paid $,Day's Gain $,Total Gain $,Total Gain %
QZFA,71.20,0.25,.35,100.0000,62.10,25.00,910.00,14.66
QZFB,44.30,-0.10,-.23,200.0000,39.25,-20.00,1010.00,12.87
TOTAL,,,,,,5.00,1920.00,13.68
""";


    // Invalid because neither of the two required columns is present.
    public static readonly string invalidEtradeMissingBothRequiredColumns = """
View Summary - All Positions

Last Price $,Change $,Change %,Quantity,Price Paid $,Day's Gain $,Total Gain $,Total Gain %
52.10,.14,.27,100.0000,45.00,14.00,710.00,15.78
78.30,-.21,-.27,50.0000,69.00,-10.50,465.00,13.48
""";


    // Invalid because an ordinary position row has an empty Symbol field.
    // Both Symbol and Value $ must be populated for every asset.
    public static readonly string invalidEtradePositionMissingSymbol = """
View Summary - All Positions

Symbol,Last Price $,Change $,Change %,Quantity,Price Paid $,Day's Gain $,Total Gain $,Total Gain %,Value $
QZHA,55.00,0.20,.37,100.0000,47.00,20.00,800.00,17.02,5500.00
,32.50,-0.05,-.15,200.0000,28.40,-10.00,820.00,14.44,6500.00
QZHB,90.00,0.60,.67,50.0000,81.00,30.00,450.00,11.11,4500.00
TOTAL,,,,,,40.00,2070.00,14.34,16500.00
""";


    // Invalid because an ordinary position row has an empty Value $ field.
    public static readonly string invalidEtradePositionMissingValue = """
View Summary - All Positions

Symbol,Last Price $,Change $,Change %,Quantity,Price Paid $,Day's Gain $,Total Gain $,Total Gain %,Value $
QZJA,61.25,0.15,.25,100.0000,54.00,15.00,725.00,13.43,6125.00
QZJB,84.10,-0.20,-.24,75.0000,78.00,-15.00,457.50,7.82,
QZJC,42.00,0.11,.26,50.0000,36.00,5.50,300.00,16.67,2100.00
TOTAL,,,,,,5.50,1482.50,11.36,14532.50
""";


    // Invalid because a position's Value $ field is populated but cannot
    // be parsed as a monetary value.
    public static readonly string invalidEtradePositionNonNumericValue = """
View Summary - All Positions

Symbol,Last Price $,Change $,Change %,Quantity,Price Paid $,Day's Gain $,Total Gain $,Total Gain %,Value $
QZKA,28.75,0.08,.28,200.0000,24.00,16.00,950.00,19.79,5750.00
QZKB,93.40,-0.30,-.32,80.0000,85.00,-24.00,672.00,9.88,NOT_A_VALUE
TOTAL,,,,,,-8.00,1622.00,14.00,13222.00
""";


    // Invalid because there is no TOTAL row against which the sum of the
    // individual position values can be validated.
    public static readonly string invalidEtradeMissingTotalRow = """
View Summary - All Positions

Symbol,Last Price $,Change $,Change %,Quantity,Price Paid $,Day's Gain $,Total Gain $,Total Gain %,Value $
QZNA,47.50,0.20,.42,100.0000,41.00,20.00,650.00,15.85,4750.00
QZNB,68.25,-0.13,-.19,100.0000,60.00,-13.00,825.00,13.75,6825.00

Generated at Aug 20 2026 01:04 PM ET
""";


    // Invalid because the TOTAL row exists, but its Value $ field is empty.
    public static readonly string invalidEtradeTotalMissingValue = """
View Summary - All Positions

Symbol,Last Price $,Change $,Change %,Quantity,Price Paid $,Day's Gain $,Total Gain $,Total Gain %,Value $
QZPA,73.00,0.10,.14,100.0000,65.00,10.00,800.00,12.31,7300.00
QZPB,39.50,-0.05,-.13,100.0000,35.00,-5.00,450.00,12.86,3950.00
TOTAL,,,,,,5.00,1250.00,12.50,
""";


    // Invalid because the TOTAL row's Value $ field is not numeric.
    public static readonly string invalidEtradeTotalNonNumericValue = """
View Summary - All Positions

Symbol,Last Price $,Change $,Change %,Quantity,Price Paid $,Day's Gain $,Total Gain $,Total Gain %,Value $
QZQA,66.00,0.30,.46,100.0000,58.00,30.00,800.00,13.79,6600.00
QZQB,41.50,-0.10,-.24,100.0000,38.00,-10.00,350.00,9.21,4150.00
TOTAL,,,,,,20.00,1150.00,11.98,INVALID
""";


    // Invalid because the individual position Value $ fields sum to
    // $14,250.00, while the TOTAL row incorrectly reports $14,249.99.
    public static readonly string invalidEtradeIncorrectTotal = """
View Summary - All Positions

Symbol,Last Price $,Change $,Change %,Quantity,Price Paid $,Day's Gain $,Total Gain $,Total Gain %,Value $
QZSA,72.50,0.17,.24,100.0000,64.00,17.00,850.00,13.28,7250.00
QZSB,35.00,-0.08,-.23,200.0000,31.00,-16.00,800.00,12.90,7000.00
TOTAL,,,,,,1.00,1650.00,13.09,14249.99
""";


    // Invalid because one malformed position has no Symbol. The remaining
    // valid positions happen to sum exactly to the reported TOTAL, so a
    // parser that silently skips the malformed row would incorrectly accept
    // the file. The actual sum of all three rows is $13,750.00, whereas
    // TOTAL reports $13,250.00.
    public static readonly string invalidEtradeMalformedRowCannotBeSkipped = """
View Summary - All Positions

Symbol,Last Price $,Change $,Change %,Quantity,Price Paid $,Day's Gain $,Total Gain $,Total Gain %,Value $
QZUA,80.00,0.20,.25,100.0000,72.00,20.00,800.00,11.11,8000.00
,25.00,0.00,.00,20.0000,24.00,0.00,20.00,4.17,500.00
QZUB,52.50,-0.10,-.19,100.0000,47.00,-10.00,550.00,11.70,5250.00
TOTAL,,,,,,10.00,1350.00,11.34,13250.00
""";
}



