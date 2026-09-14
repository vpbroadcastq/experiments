namespace a_alloc.Tests;

public class EtfcImporterTests
{
    public static IEnumerable<object[]> ValidCases()
    {
        yield return new object[] { TestData.validEtradeExampleA, TestData.resultValidEtradeExampleA };
        yield return new object[] { TestData.validEtradeExampleB, TestData.resultValidEtradeExampleB };
        yield return new object[] { TestData.validEtradeExampleC, TestData.resultValidEtradeExampleC };
        yield return new object[] { TestData.validEtradeExampleD, TestData.resultValidEtradeExampleD };
    }

    public static IEnumerable<object[]> InvalidCases()
    {
        yield return new object[] { TestData.invalidEtradeNoPositionsTable };
        yield return new object[] { TestData.invalidEtradeMissingSymbolColumn };
        yield return new object[] { TestData.invalidEtradeMissingValueColumn };
        yield return new object[] { TestData.invalidEtradeMissingBothRequiredColumns };
        yield return new object[] { TestData.invalidEtradePositionMissingSymbol };
        yield return new object[] { TestData.invalidEtradePositionMissingValue };
        yield return new object[] { TestData.invalidEtradePositionNonNumericValue };
        yield return new object[] { TestData.invalidEtradeMissingTotalRow };
        yield return new object[] { TestData.invalidEtradeTotalMissingValue };
        yield return new object[] { TestData.invalidEtradeTotalNonNumericValue };
        yield return new object[] { TestData.invalidEtradeIncorrectTotal };
        yield return new object[] { TestData.invalidEtradeMalformedRowCannotBeSkipped };
    }

    [Theory]
    [MemberData(nameof(ValidCases))]
    public void Import_ValidData_ReturnsExpectedAssets(
        string payload,
        List<Utils.Asset> expected)
    {
        List<Utils.Asset>? actual = EtfcImporter.Import(ToLines(payload));

        Assert.NotNull(actual);
        AssertAssetsEqual(expected, actual!);
    }

    [Theory]
    [MemberData(nameof(InvalidCases))]
    public void Import_InvalidData_ReturnsNull(string payload)
    {
        List<Utils.Asset>? actual = EtfcImporter.Import(ToLines(payload));

        Assert.Null(actual);
    }

    private static string[] ToLines(string payload)
    {
        return payload
            .Split('\n')
            .Select(line => line.TrimEnd('\r'))
            .ToArray();
    }

    private static void AssertAssetsEqual(List<Utils.Asset> expected, List<Utils.Asset> actual)
    {
        List<Utils.Asset> expectedSorted = expected
            .OrderBy(asset => asset.symbol, StringComparer.Ordinal)
            .ToList();

        List<Utils.Asset> actualSorted = actual
            .OrderBy(asset => asset.symbol, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(expectedSorted.Count, actualSorted.Count);

        for (int i = 0; i < expectedSorted.Count; ++i)
        {
            Assert.Equal(expectedSorted[i].symbol, actualSorted[i].symbol);
            Assert.InRange(Math.Abs(expectedSorted[i].value - actualSorted[i].value), 0.0, 0.000001);
        }
    }
}
