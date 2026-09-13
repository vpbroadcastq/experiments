namespace a_alloc.Tests;

public class TiaaImporterTests
{
    public static IEnumerable<object[]> ValidCases()
    {
        yield return new object[] { TestData.validExampleA, TestData.resultValidExampleA };
        yield return new object[] { TestData.validExampleB, TestData.resultValidExampleB };
        yield return new object[] { TestData.validExampleC, TestData.resultValidExampleC };
        yield return new object[] { TestData.validExampleD, TestData.resultValidExampleD };
        yield return new object[] { TestData.validExampleE, TestData.resultValidExampleE };
    }

    public static IEnumerable<object[]> InvalidCases()
    {
        yield return new object[] { TestData.invalidNoInvestmentTable };
        yield return new object[] { TestData.invalidMissingInvestmentsColumn };
        yield return new object[] { TestData.invalidMissingTotalValueColumn };
        yield return new object[] { TestData.invalidMissingBothRequiredColumns };
        yield return new object[] { TestData.invalidAssetMissingDescription };
        yield return new object[] { TestData.invalidAssetMissingTotalValue };
        yield return new object[] { TestData.invalidAssetNonNumericTotalValue };
        yield return new object[] { TestData.invalidMissingAllInvestmentsRow };
        yield return new object[] { TestData.invalidAllInvestmentsMissingTotalValue };
        yield return new object[] { TestData.invalidAllInvestmentsNonNumericTotal };
        yield return new object[] { TestData.invalidIncorrectAllInvestmentsTotal };
        yield return new object[] { TestData.invalidTotalOmitsOneAccount };
        yield return new object[] { TestData.invalidMalformedRowCannotBeSkipped };
    }

    [Theory]
    [MemberData(nameof(ValidCases))]
    public void Import_ValidData_ReturnsExpectedAssets(
        string payload,
        List<Utils.Asset> expected)
    {
        List<Utils.Asset>? actual = TiaaImporter.Import(ToLines(payload));

        Assert.NotNull(actual);
        AssertAssetsEqual(expected, actual!);
    }

    [Theory]
    [MemberData(nameof(InvalidCases))]
    public void Import_InvalidData_ReturnsNull(string payload)
    {
        List<Utils.Asset>? actual = TiaaImporter.Import(ToLines(payload));

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
