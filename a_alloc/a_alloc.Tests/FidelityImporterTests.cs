namespace a_alloc.Tests;

public class FidelityImporterTests
{
    public static IEnumerable<object[]> ValidCases()
    {
        yield return new object[] { TestData.validFidelityExampleA, TestData.resultValidFidelityExampleA };
        yield return new object[] { TestData.validFidelityExampleB, TestData.resultValidFidelityExampleB };
        yield return new object[] { TestData.validFidelityExampleC, TestData.resultValidFidelityExampleC };
    }

    public static IEnumerable<object[]> InvalidCases()
    {
        yield return new object[] { TestData.invalidFidelityNoTable };
        yield return new object[] { TestData.invalidFidelityMissingSymbolColumn };
        yield return new object[] { TestData.invalidFidelityMissingDescriptionColumn };
        yield return new object[] { TestData.invalidFidelityMissingCurrentValueColumn };
        yield return new object[] { TestData.invalidFidelityMissingAllRequiredColumns };
        yield return new object[] { TestData.invalidFidelityMissingAssetIdentifier };
        yield return new object[] { TestData.invalidFidelityMissingCurrentValue };
        yield return new object[] { TestData.invalidFidelityNonNumericCurrentValue };
        yield return new object[] { TestData.invalidFidelityMalformedRowCannotBeSkipped };
    }

    [Theory]
    [MemberData(nameof(ValidCases))]
    public void Import_ValidData_ReturnsExpectedAssets(
        string payload,
        List<Utils.Asset> expected)
    {
        List<Utils.Asset>? actual = FidelityImporter.Import(ToLines(payload));

        Assert.NotNull(actual);
        AssertAssetsEqual(expected, actual!);
    }

    [Theory]
    [MemberData(nameof(InvalidCases))]
    public void Import_InvalidData_ReturnsNull(string payload)
    {
        List<Utils.Asset>? actual = FidelityImporter.Import(ToLines(payload));

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
