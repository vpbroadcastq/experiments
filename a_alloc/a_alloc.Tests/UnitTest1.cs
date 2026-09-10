namespace a_alloc.Tests;

public class SplitterTests
{
    private static List<string> Collect(string payload, char delim = ',')
    {
        var splitter = new Utils.Splitter(delim, payload);
        List<string> result = new();

        while (!splitter.Finished())
        {
            result.Add(splitter.Current().ToString());
            if (!splitter.GoNext())
            {
                break;
            }
        }

        return result;
    }

    [Fact]
    public void EmptyString_HasNoSegments()
    {
        var splitter = new Utils.Splitter(',', "");

        Assert.True(splitter.Finished());
        Assert.False(splitter.GoNext());
        Assert.Empty(Collect(""));
    }

    [Theory]
    [InlineData("a")]
    [InlineData("abc")]
    [InlineData("alpha")]
    public void SingleTokenWithoutDelimiter_ProducesOneSegment(string payload)
    {
        Assert.Equal(new[] { payload }, Collect(payload));
    }

    [Theory]
    [InlineData("a,b", new[] { "a", "b" })]
    [InlineData("a,b,c", new[] { "a", "b", "c" })]
    [InlineData("alpha,beta,gamma", new[] { "alpha", "beta", "gamma" })]
    public void MultipleTokensSeparatedByDelimiter_ProducesEachSegment(string payload, string[] expected)
    {
        Assert.Equal(expected, Collect(payload));
    }

    [Theory]
    [InlineData(",a", new[] { "", "a" })]
    [InlineData(",a,b", new[] { "", "a", "b" })]
    [InlineData(",,,a", new[] { "", "", "", "a" })]
    public void LeadingDelimiters_KeepTheLeadingEmptySegment(string payload, string[] expected)
    {
        Assert.Equal(expected, Collect(payload));
    }

    [Theory]
    [InlineData("a,", new[] { "a" })]
    [InlineData("a,b,", new[] { "a", "b" })]
    [InlineData("a,,,", new[] { "a", "", "" })]
    public void TrailingDelimiters_DoNotProduceAnExtraTrailingEmptySegment(string payload, string[] expected)
    {
        Assert.Equal(expected, Collect(payload));
    }

    [Theory]
    [InlineData("a,,b", new[] { "a", "", "b" })]
    [InlineData("a,,,b", new[] { "a", "", "", "b" })]
    [InlineData(",,a,,b,,", new[] { "", "", "a", "", "b", "" })]
    public void RepeatedAndAdjacentDelimiters_CreateEmptySegments(string payload, string[] expected)
    {
        Assert.Equal(expected, Collect(payload));
    }

    [Theory]
    [InlineData(",")]
    [InlineData(",,")]
    [InlineData("...", '.')] // extra coverage for a non-comma delimiter
    public void DelimiterOnlyInput_ProducesOnlyEmptySegmentsUntilFinished(string payload, char delim = ',')
    {
        var splitter = new Utils.Splitter(delim, payload);
        List<string> result = new();

        while (!splitter.Finished())
        {
            result.Add(splitter.Current().ToString());
            if (!splitter.GoNext())
            {
                break;
            }
        }

        Assert.NotEmpty(result);
        Assert.All(result, segment => Assert.Equal(string.Empty, segment));
        Assert.True(splitter.Finished());
    }

    [Fact]
    public void GoNext_ReturnsFalseWhenThereAreNoMoreSegments()
    {
        var splitter = new Utils.Splitter(',', "a");

        Assert.Equal("a", splitter.Current().ToString());
        Assert.True(splitter.GoNext() == false);
        Assert.True(splitter.Finished());
    }

    [Fact]
    public void DelimiterCanBeUserSpecified()
    {
        var splitter = new Utils.Splitter('|', "one|two|three");
        List<string> result = new();

        while (!splitter.Finished())
        {
            result.Add(splitter.Current().ToString());
            if (!splitter.GoNext())
            {
                break;
            }
        }

        Assert.Equal(new[] { "one", "two", "three" }, result);
    }
}
