namespace a_alloc.Tests;

public class RuleViolationTests
{
    [Fact]
    public void ViloatesExhaustiveRule_EmptyCategorySet_IsViolation()
    {
        Assert.True(Utils.ViloatesExhaustiveRule(
            Array.Empty<int>(),
            new[] { 1, 2 }));
    }

    [Fact]
    public void ViloatesExhaustiveRule_EmptyRule_IsViolation()
    {
        Assert.True(Utils.ViloatesExhaustiveRule(
            new[] { 1, 2 },
            Array.Empty<int>()));
    }

    [Fact]
    public void ViloatesExhaustiveRule_BothInputsEmpty_IsViolation()
    {
        Assert.True(Utils.ViloatesExhaustiveRule(
            Array.Empty<int>(),
            Array.Empty<int>()));
    }

    [Fact]
    public void ViloatesExhaustiveRule_NoCategoryMatchesRule_IsViolation()
    {
        Assert.True(Utils.ViloatesExhaustiveRule(
            new[] { 1, 2 },
            new[] { 3, 4 }));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(7)]
    public void ViloatesExhaustiveRule_AnyCategoryMatch_IsNotViolation(int matchingCategory)
    {
        Assert.False(Utils.ViloatesExhaustiveRule(
            new[] { 1, matchingCategory, 3 },
            new[] { 2, matchingCategory, 4 }));
    }

    [Fact]
    public void ViloatesExclusiveRule_EmptyCategorySet_IsNotViolation()
    {
        Utils.RuleViolationExclusive result = Utils.ViloatesExclusiveRule(
            Array.Empty<int>(),
            new[] { 1, 2 });

        Assert.True(result.IsEmpty());
    }

    [Fact]
    public void ViloatesExclusiveRule_EmptyRule_IsNotViolation()
    {
        Utils.RuleViolationExclusive result = Utils.ViloatesExclusiveRule(
            new[] { 1, 2 },
            Array.Empty<int>());

        Assert.True(result.IsEmpty());
    }

    [Fact]
    public void ViloatesExclusiveRule_BothInputsEmpty_IsNotViolation()
    {
        Utils.RuleViolationExclusive result = Utils.ViloatesExclusiveRule(
            Array.Empty<int>(),
            Array.Empty<int>());

        Assert.True(result.IsEmpty());
    }

    [Fact]
    public void ViloatesExclusiveRule_OneCategoryMatch_IsNotViolation()
    {
        Utils.RuleViolationExclusive result = Utils.ViloatesExclusiveRule(
            new[] { 1, 3 },
            new[] { 2, 3 });

        Assert.True(result.IsEmpty());
    }

    [Fact]
    public void ViloatesExclusiveRule_NoCategoryMatchesRule_IsNotViolation()
    {
        Utils.RuleViolationExclusive result = Utils.ViloatesExclusiveRule(
            new[] { 1, 2 },
            new[] { 3, 4 });

        Assert.True(result.IsEmpty());
    }

    [Fact]
    public void ViloatesExclusiveRule_TwoCategoryMatches_IsViolation()
    {
        Utils.RuleViolationExclusive result = Utils.ViloatesExclusiveRule(
            new[] { 1, 2, 4 },
            new[] { 2, 3, 4 });

        Assert.False(result.IsEmpty());
        Assert.Equal(2, result.idxa);
        Assert.Equal(4, result.idxb);
    }

    [Fact]
    public void ViloatesExclusiveRule_RepeatedMatchingCategory_IsViolation()
    {
        Utils.RuleViolationExclusive result = Utils.ViloatesExclusiveRule(
            new[] { 1, 2, 2 },
            new[] { 2, 3 });

        Assert.False(result.IsEmpty());
        Assert.Equal(2, result.idxa);
        Assert.Equal(2, result.idxb);
    }

    [Fact]
    public void ViloatesExclusiveRule_MoreThanTwoCategoryMatches_ReturnsFirstTwoMatches()
    {
        Utils.RuleViolationExclusive result = Utils.ViloatesExclusiveRule(
            new[] { 1, 2, 3, 4 },
            new[] { 2, 3, 4 });

        Assert.False(result.IsEmpty());
        Assert.Equal(2, result.idxa);
        Assert.Equal(3, result.idxb);
    }
}
