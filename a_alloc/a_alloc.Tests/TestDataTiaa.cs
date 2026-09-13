



internal static class TestData
{
    //
    // Valid
    //

    // HAHA occurs in two separate accounts and must be combined into a single Asset.
    public static readonly string validExampleA = """
Data As of 08/14/2026,
Your Investments,Total Value,Last Price,Quantity,Change since last close ($)*,Change since last close (%)*,Cost basis ‡,
UNIVERSITY OF WHATEVER SYSTEM 457B DEFERRED COMPENSATION PLAN,
Total balance: $37939.11,
Crappy global active fund R3 (SYMB),$25662.38,$493.1753,52.0350,-$67.91,-0.26%,$9272.26,
CREF Inflation-Linked Bond R3 (HAHA),$12276.73,$92.2012,133.1515,-$8.21,-0.07%,$9264.00,
UNIVERSITY OF WHATEVER SYSTEM AMAZING RETIREMENT PLAN,
Total balance: $65779.07,
CREF Inflation-Linked Bond R3 (HAHA),$24528.42,$92.2012,266.0315,-$16.42,-0.07%,$18414.09,
Nuveen International Equity Index Fund R6,$41250.65,$32.0200,1288.2778,$0.00,0.00%,$24632.48,
All Investments,$103718.18, , ,-$92.54,-0.09%, ,
*Reflects the change between the selected time period and the current value. Can include contributions withdrawals and loan repayments.,
‡ Cost basis provided in your retirement account is for informational purposes only and should not be used for tax purposes since retirement plan investments do not receive capital gains tax treatment.,
""";

    public static readonly List<Utils.Asset> resultValidExampleA =
        new List<Utils.Asset>
        {
            new Utils.Asset("SYMB", 25662.38),
            new Utils.Asset("HAHA", 36805.15),
            new Utils.Asset("Nuveen International Equity Index Fund R6", 41250.65)
        };


    // A simple single-account statement containing one investment with a
    // ticker and one investment without a ticker.
    public static readonly string validExampleB = """
Data As of 08/14/2026,
Your Investments,Total Value,Last Price,Quantity,Change since last close ($)*,Change since last close (%)*,Cost basis ‡,
EXAMPLE RETIREMENT PLAN,
Total balance: $2000.00,
Fidelity 500 Index Fund (FXAIX),$1234.56,$211.25,5.8441,$1.23,0.10%,$900.00,
Stable Value Fund,$765.44,$1.0000,765.4400,$0.00,0.00%,$765.44,
All Investments,$2000.00, , ,$1.23,0.06%, ,
""";

    public static readonly List<Utils.Asset> resultValidExampleB =
        new List<Utils.Asset>
        {
            new Utils.Asset("FXAIX", 1234.56),
            new Utils.Asset("Stable Value Fund", 765.44)
        };


    // The same ticker and the same description-only investment both occur
    // in multiple accounts. Each must be globally combined.
    public static readonly string validExampleC = """
Data As of 08/14/2026,
Your Investments,Total Value,Last Price,Quantity,Change since last close ($)*,Change since last close (%)*,Cost basis ‡,
PLAN ONE,
Total balance: $425.00,
CREF Inflation-Linked Bond R3 (QCILIX),$100.00,$92.2012,1.0846,$0.00,0.00%,$90.00,
Nuveen International Equity Index Fund R6,$300.00,$32.0200,9.3691,$0.00,0.00%,$250.00,
CREF Global Equities R3 (QCGLIX),$25.00,$493.1753,0.0507,$0.00,0.00%,$20.00,
PLAN TWO,
Total balance: $700.00,
CREF Inflation-Linked Bond R3 (QCILIX),$250.00,$92.2012,2.7115,$0.00,0.00%,$225.00,
Nuveen International Equity Index Fund R6,$450.00,$32.0200,14.0537,$0.00,0.00%,$400.00,
All Investments,$1125.00, , ,$0.00,0.00%, ,
""";

    public static readonly List<Utils.Asset> resultValidExampleC =
        new List<Utils.Asset>
        {
            new Utils.Asset("QCILIX", 350.00),
            new Utils.Asset("Nuveen International Equity Index Fund R6", 750.00),
            new Utils.Asset("QCGLIX", 25.00)
        };


    // Multiple accounts with different investments. The individual account
    // balances are irrelevant to the returned result; only the investments
    // themselves are globally accumulated.
    public static readonly string validExampleD = """
Data As of 08/14/2026,
Your Investments,Total Value,Last Price,Quantity,Change since last close ($)*,Change since last close (%)*,Cost basis ‡,
PLAN ALPHA,
Total balance: $1500.25,
CREF Stock R3 (QCSTIX),$1000.25,$100.00,10.0025,$0.00,0.00%,$800.00,
Money Market Account,$500.00,$1.0000,500.0000,$0.00,0.00%,$500.00,
PLAN BETA,
Total balance: $850.75,
CREF Growth R3 (QCGRIX),$850.75,$50.00,17.0150,$0.00,0.00%,$700.00,
All Investments,$2351.00, , ,$0.00,0.00%, ,
""";

    public static readonly List<Utils.Asset> resultValidExampleD =
        new List<Utils.Asset>
        {
            new Utils.Asset("QCSTIX", 1000.25),
            new Utils.Asset("Money Market Account", 500.00),
            new Utils.Asset("QCGRIX", 850.75)
        };


    // Valid statement containing only description-based investments.
    public static readonly string validExampleE = """
Data As of 08/14/2026,
Your Investments,Total Value,Last Price,Quantity,Change since last close ($)*,Change since last close (%)*,Cost basis ‡,
EXAMPLE PLAN,
Total balance: $3000.00,
Nuveen International Equity Index Fund R6,$1250.00,$32.0200,39.0381,$0.00,0.00%,$1000.00,
TIAA Real Estate Account,$1000.00,$500.0000,2.0000,$0.00,0.00%,$800.00,
Guaranteed Fixed Account,$750.00,$1.0000,750.0000,$0.00,0.00%,$750.00,
All Investments,$3000.00, , ,$0.00,0.00%, ,
""";

    public static readonly List<Utils.Asset> resultValidExampleE =
        new List<Utils.Asset>
        {
            new Utils.Asset("Nuveen International Equity Index Fund R6", 1250.00),
            new Utils.Asset("TIAA Real Estate Account", 1000.00),
            new Utils.Asset("Guaranteed Fixed Account", 750.00)
        };


    //
    // Invalid
    //

    // Invalid because there is no investment table at all.
    public static readonly string invalidNoInvestmentTable = """
Data As of 08/14/2026,
UNIVERSITY OF TEXAS SYSTEM 457B DEFERRED COMPENSATION PLAN,
Total balance: $37939.11,
This file contains no investment table.,
""";


    // Invalid because the investment table is missing the "Your Investments"
    // column, which contains the description/ticker used to identify an asset.
    public static readonly string invalidMissingInvestmentsColumn = """
Data As of 08/14/2026,
Total Value,Last Price,Quantity,Change since last close ($)*,Change since last close (%)*,Cost basis ‡,
EXAMPLE PLAN,
Total balance: $1000.00,
$1000.00,$50.00,20.0000,$0.00,0.00%,$900.00,
All Investments,$1000.00, , ,$0.00,0.00%, ,
""";


    // Invalid because the investment table is missing the "Total Value"
    // column, which contains the monetary value required for each asset.
    public static readonly string invalidMissingTotalValueColumn = """
Data As of 08/14/2026,
Your Investments,Last Price,Quantity,Change since last close ($)*,Change since last close (%)*,Cost basis ‡,
EXAMPLE PLAN,
Total balance: $1000.00,
CREF Global Equities R3 (QCGLIX),$493.1753,2.0277,$0.00,0.00%,$900.00,
All Investments,$1000.00, , ,$0.00,0.00%, ,
""";


    // Invalid because both required columns -- "Your Investments" and
    // "Total Value" -- are absent from the table header.
    public static readonly string invalidMissingBothRequiredColumns = """
Data As of 08/14/2026,
Last Price,Quantity,Change since last close ($)*,Change since last close (%)*,Cost basis ‡,
EXAMPLE PLAN,
Total balance: $1000.00,
$493.1753,2.0277,$0.00,0.00%,$900.00,
All Investments,$1000.00, , ,$0.00,0.00%, ,
""";


    // Invalid because an investment row has no value in the "Your Investments"
    // column, so there is neither a ticker nor a description with which to
    // identify the asset.
    public static readonly string invalidAssetMissingDescription = """
Data As of 08/14/2026,
Your Investments,Total Value,Last Price,Quantity,Change since last close ($)*,Change since last close (%)*,Cost basis ‡,
EXAMPLE PLAN,
Total balance: $1000.00,
,$400.00,$20.00,20.0000,$0.00,0.00%,$350.00,
CREF Global Equities R3 (QCGLIX),$600.00,$493.1753,1.2166,$0.00,0.00%,$500.00,
All Investments,$1000.00, , ,$0.00,0.00%, ,
""";


    // Invalid because an investment row has no value in the "Total Value"
    // column. The parser must not silently skip this row.
    public static readonly string invalidAssetMissingTotalValue = """
Data As of 08/14/2026,
Your Investments,Total Value,Last Price,Quantity,Change since last close ($)*,Change since last close (%)*,Cost basis ‡,
EXAMPLE PLAN,
Total balance: $1000.00,
CREF Inflation-Linked Bond R3 (QCILIX),,$92.2012,4.3383,$0.00,0.00%,$350.00,
CREF Global Equities R3 (QCGLIX),$600.00,$493.1753,1.2166,$0.00,0.00%,$500.00,
All Investments,$1000.00, , ,$0.00,0.00%, ,
""";


    // Invalid because an investment's "Total Value" field is present but
    // cannot be parsed as a monetary value.
    public static readonly string invalidAssetNonNumericTotalValue = """
Data As of 08/14/2026,
Your Investments,Total Value,Last Price,Quantity,Change since last close ($)*,Change since last close (%)*,Cost basis ‡,
EXAMPLE PLAN,
Total balance: $1000.00,
CREF Inflation-Linked Bond R3 (QCILIX),NOT_A_VALUE,$92.2012,4.3383,$0.00,0.00%,$350.00,
CREF Global Equities R3 (QCGLIX),$600.00,$493.1753,1.2166,$0.00,0.00%,$500.00,
All Investments,$1000.00, , ,$0.00,0.00%, ,
""";


    // Invalid because there is no "All Investments" row against which the
    // sum of the individual investments can be reconciled.
    public static readonly string invalidMissingAllInvestmentsRow = """
Data As of 08/14/2026,
Your Investments,Total Value,Last Price,Quantity,Change since last close ($)*,Change since last close (%)*,Cost basis ‡,
EXAMPLE PLAN,
Total balance: $1000.00,
CREF Inflation-Linked Bond R3 (QCILIX),$400.00,$92.2012,4.3383,$0.00,0.00%,$350.00,
CREF Global Equities R3 (QCGLIX),$600.00,$493.1753,1.2166,$0.00,0.00%,$500.00,
""";


    // Invalid because the "All Investments" row exists but its "Total Value"
    // field is empty, so reconciliation cannot be performed.
    public static readonly string invalidAllInvestmentsMissingTotalValue = """
Data As of 08/14/2026,
Your Investments,Total Value,Last Price,Quantity,Change since last close ($)*,Change since last close (%)*,Cost basis ‡,
EXAMPLE PLAN,
Total balance: $1000.00,
CREF Inflation-Linked Bond R3 (QCILIX),$400.00,$92.2012,4.3383,$0.00,0.00%,$350.00,
CREF Global Equities R3 (QCGLIX),$600.00,$493.1753,1.2166,$0.00,0.00%,$500.00,
All Investments,, , ,$0.00,0.00%, ,
""";


    // Invalid because the "All Investments" total is not numeric.
    public static readonly string invalidAllInvestmentsNonNumericTotal = """
Data As of 08/14/2026,
Your Investments,Total Value,Last Price,Quantity,Change since last close ($)*,Change since last close (%)*,Cost basis ‡,
EXAMPLE PLAN,
Total balance: $1000.00,
CREF Inflation-Linked Bond R3 (QCILIX),$400.00,$92.2012,4.3383,$0.00,0.00%,$350.00,
CREF Global Equities R3 (QCGLIX),$600.00,$493.1753,1.2166,$0.00,0.00%,$500.00,
All Investments,INVALID, , ,$0.00,0.00%, ,
""";


    // Invalid because the individual investments sum to $1000.00, whereas
    // the "All Investments" row incorrectly reports $999.99.
    public static readonly string invalidIncorrectAllInvestmentsTotal = """
Data As of 08/14/2026,
Your Investments,Total Value,Last Price,Quantity,Change since last close ($)*,Change since last close (%)*,Cost basis ‡,
EXAMPLE PLAN,
Total balance: $1000.00,
CREF Inflation-Linked Bond R3 (QCILIX),$400.00,$92.2012,4.3383,$0.00,0.00%,$350.00,
CREF Global Equities R3 (QCGLIX),$600.00,$493.1753,1.2166,$0.00,0.00%,$500.00,
All Investments,$999.99, , ,$0.00,0.00%, ,
""";


    // Invalid because the incorrect total is caused by failing to include
    // investments from all accounts. The assets actually sum to $1500.00,
    // but "All Investments" reports only the second account's $900.00.
    public static readonly string invalidTotalOmitsOneAccount = """
Data As of 08/14/2026,
Your Investments,Total Value,Last Price,Quantity,Change since last close ($)*,Change since last close (%)*,Cost basis ‡,
PLAN ONE,
Total balance: $600.00,
CREF Global Equities R3 (QCGLIX),$600.00,$493.1753,1.2166,$0.00,0.00%,$500.00,
PLAN TWO,
Total balance: $900.00,
CREF Inflation-Linked Bond R3 (QCILIX),$400.00,$92.2012,4.3383,$0.00,0.00%,$350.00,
Nuveen International Equity Index Fund R6,$500.00,$32.0200,15.6152,$0.00,0.00%,$450.00,
All Investments,$900.00, , ,$0.00,0.00%, ,
""";


    // Invalid because one row among otherwise valid investment rows has an
    // empty description. The fact that all of the identifiable rows happen
    // to add up to the reported total must not cause the malformed row to
    // be silently ignored.
    public static readonly string invalidMalformedRowCannotBeSkipped = """
Data As of 08/14/2026,
Your Investments,Total Value,Last Price,Quantity,Change since last close ($)*,Change since last close (%)*,Cost basis ‡,
EXAMPLE PLAN,
Total balance: $1000.00,
CREF Global Equities R3 (QCGLIX),$400.00,$493.1753,0.8111,$0.00,0.00%,$350.00,
,$50.00,$1.0000,50.0000,$0.00,0.00%,$50.00,
CREF Inflation-Linked Bond R3 (QCILIX),$600.00,$92.2012,6.5075,$0.00,0.00%,$500.00,
All Investments,$1000.00, , ,$0.00,0.00%, ,
""";
}



