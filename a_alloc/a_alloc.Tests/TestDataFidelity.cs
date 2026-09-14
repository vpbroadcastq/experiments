
internal static partial class TestData
{
    //
    // Fidelity - Valid
    //

    // Basic valid Fidelity file. Two assets have no Symbol and therefore use
    // Description; one has a Symbol and therefore uses Symbol.
    public static readonly string validFidelityExampleA = """
    Account number,Account name,Symbol,Description,CUSIP,Current value,Total gain/loss %,Today's gain/loss %,Security type,Security subtype
    31482,BLUE RIDGE TECHNOLOGIES 401K,,SUMMIT CORE BOND POOL,,$18432.77,+6.14%,-0.21%,Custom Fund,,
    31482,BLUE RIDGE TECHNOLOGIES 401K,,GRANITE SMALL CAP VALUE TRUST,,$27105.42,+19.73%,+0.42%,Custom Fund,,
    31482,BLUE RIDGE TECHNOLOGIES 401K,49123A701,AURORA LARGE CAP INDEX TRUST,49123A701,$33618.09,+11.26%,-0.08%,,,

    "This information is provided for informational purposes only."

    "Date downloaded Aug-21-2026 9:15 a.m ET"
    """;

    public static readonly List<Utils.Asset> resultValidFidelityExampleA =
        new List<Utils.Asset>
        {
            new Utils.Asset("SUMMIT CORE BOND POOL", 18432.77),
            new Utils.Asset("GRANITE SMALL CAP VALUE TRUST", 27105.42),
            new Utils.Asset("49123A701", 33618.09)
        };


    // Valid because the required columns need to exist, but do not need to occur
    // at the same offsets as in the sample Fidelity export.
    public static readonly string validFidelityExampleB = """
    Description,Current value,Account name,Security type,Symbol,Account number,CUSIP,Total gain/loss %,Today's gain/loss %,Security subtype
    ORCHARD INTERNATIONAL EQUITY FUND,$512.34,RIVERSTONE INDUSTRIES 403B,Custom Fund,,55291,,+4.17%,-0.12%,
    NORTH COAST TREASURY INDEX,$9012.67,RIVERSTONE INDUSTRIES 403B,,61872C309,55291,61872C309,+1.92%,+0.03%,,

    "End of positions."
    """;

    public static readonly List<Utils.Asset> resultValidFidelityExampleB =
        new List<Utils.Asset>
        {
            new Utils.Asset("ORCHARD INTERNATIONAL EQUITY FUND", 512.34),
            new Utils.Asset("61872C309", 9012.67)
        };


    // Valid because Description is permitted to be empty for a particular row
    // when Symbol is present. Symbol is the identifier whenever it is nonempty.
    public static readonly string validFidelityExampleC = """
    Account number,Account name,Symbol,Description,CUSIP,Current value,Total gain/loss %,Today's gain/loss %,Security type,Security subtype
    62173,PINE VALLEY EMPLOYEE SAVINGS PLAN,90317D804,,90317D804,$777.77,+5.40%,-0.05%,,,
    62173,PINE VALLEY EMPLOYEE SAVINGS PLAN,,MEADOW CONSERVATIVE ALLOCATION FUND,,$2222.23,+7.13%,+0.11%,Custom Fund,,

    "End of file."
    """;

    public static readonly List<Utils.Asset> resultValidFidelityExampleC =
        new List<Utils.Asset>
        {
            new Utils.Asset("90317D804", 777.77),
            new Utils.Asset("MEADOW CONSERVATIVE ALLOCATION FUND", 2222.23)
        };


    //
    // Fidelity - Invalid
    //

    // Invalid because there is no positions table at all.
    public static readonly string invalidFidelityNoTable = """
    Account information exported Aug-21-2026.

    This file does not contain any position data.

    "End of file."
    """;


    // Invalid because the required "Symbol" column is absent, even though
    // Description is present for every investment.
    public static readonly string invalidFidelityMissingSymbolColumn = """
    Account number,Account name,Description,CUSIP,Current value,Total gain/loss %,Today's gain/loss %,Security type,Security subtype
    44218,MAPLE CREEK RETIREMENT PLAN,FOOTHILL BOND INDEX POOL,,$8350.40,+4.19%,-0.10%,Custom Fund,,
    44218,MAPLE CREEK RETIREMENT PLAN,WESTERN LARGE CAP TRUST,,$11320.75,+10.62%,+0.06%,Custom Fund,,
    """;


    // Invalid because the required "Description" column is absent, even though
    // every position happens to have a Symbol.
    public static readonly string invalidFidelityMissingDescriptionColumn = """
    Account number,Account name,Symbol,CUSIP,Current value,Total gain/loss %,Today's gain/loss %,Security type,Security subtype
    51726,STONEBRIDGE EMPLOYEE PLAN,27491F602,27491F602,$6451.88,+8.34%,-0.14%,,,
    51726,STONEBRIDGE EMPLOYEE PLAN,74128H105,74128H105,$9282.16,+15.07%,+0.22%,,,
    """;


    // Invalid because the required "Current value" column is absent.
    public static readonly string invalidFidelityMissingCurrentValueColumn = """
    Account number,Account name,Symbol,Description,CUSIP,Total gain/loss %,Today's gain/loss %,Security type,Security subtype
    63820,REDWOOD ANALYTICS SAVINGS PLAN,,LAKESHORE FIXED INCOME FUND,,+5.48%,-0.07%,Custom Fund,,
    63820,REDWOOD ANALYTICS SAVINGS PLAN,30517K901,VALLEY EQUITY INDEX,30517K901,+17.93%,+0.18%,,,
    """;


    // Invalid because all three columns required by the importer are absent.
    public static readonly string invalidFidelityMissingAllRequiredColumns = """
    Account number,Account name,CUSIP,Total gain/loss %,Today's gain/loss %,Security type,Security subtype
    77341,SILVER FIR EMPLOYEE PLAN,,+3.45%,+0.00%,Custom Fund,,
    77341,SILVER FIR EMPLOYEE PLAN,81266M304,+12.38%,-0.19%,,,
    """;


    // Invalid because this position has neither a Symbol nor a Description, so
    // there is no possible asset identifier.
    public static readonly string invalidFidelityMissingAssetIdentifier = """
    Account number,Account name,Symbol,Description,CUSIP,Current value,Total gain/loss %,Today's gain/loss %,Security type,Security subtype
    90214,MOUNTAIN VIEW RETIREMENT PROGRAM,,ASPEN INCOME TRUST,,$4312.19,+3.12%,+0.01%,Custom Fund,,
    90214,MOUNTAIN VIEW RETIREMENT PROGRAM,,,,$2875.63,+6.81%,-0.04%,Custom Fund,,
    90214,MOUNTAIN VIEW RETIREMENT PROGRAM,66412P508,FRONTIER MARKET INDEX,66412P508,$7611.28,+14.44%,+0.27%,,,
    """;


    // Invalid because a position row has an empty Current value field.
    public static readonly string invalidFidelityMissingCurrentValue = """
    Account number,Account name,Symbol,Description,CUSIP,Current value,Total gain/loss %,Today's gain/loss %,Security type,Security subtype
    15834,ALPINE DATA SYSTEMS 401K,,PRAIRIE CAPITAL PRESERVATION FUND,,$7145.91,+2.76%,+0.00%,Custom Fund,,
    15834,ALPINE DATA SYSTEMS 401K,42819R307,HIGHLAND STOCK INDEX,42819R307,,+16.50%,-0.23%,,,
    """;


    // Invalid because a position's Current value field cannot be parsed as
    // a monetary value.
    public static readonly string invalidFidelityNonNumericCurrentValue = """
    Account number,Account name,Symbol,Description,CUSIP,Current value,Total gain/loss %,Today's gain/loss %,Security type,Security subtype
    26519,IRONWOOD ENGINEERING SAVINGS PLAN,,FOREST CORE ALLOCATION FUND,,$5410.82,+5.19%,+0.04%,Custom Fund,,
    26519,IRONWOOD ENGINEERING SAVINGS PLAN,83072N406,COASTAL EQUITY TRUST,83072N406,NOT_A_VALUE,+13.71%,-0.15%,,,
    """;


    // Invalid because one malformed position lacks an identifier.
    public static readonly string invalidFidelityMalformedRowCannotBeSkipped = """
    Account number,Account name,Symbol,Description,CUSIP,Current value,Total gain/loss %,Today's gain/loss %,Security type,Security subtype
    34671,COPPER RIDGE EMPLOYEE PLAN,57218S209,SIERRA LARGE CAP INDEX,57218S209,$10250.16,+11.39%,-0.09%,,,
    34671,COPPER RIDGE EMPLOYEE PLAN,,,,$938.44,+1.15%,+0.00%,Custom Fund,,
    34671,COPPER RIDGE EMPLOYEE PLAN,,BIRCH INTERNATIONAL STOCK FUND,,$6821.37,+9.64%,+0.13%,Custom Fund,,

    "This text occurs after the positions table and is not itself a position."
    """;

}




