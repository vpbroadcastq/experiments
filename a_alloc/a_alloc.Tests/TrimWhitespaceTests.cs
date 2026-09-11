namespace a_alloc.Tests;

public class TrimWhitespaceTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t\n\r")]
    public void TrimWhitespace_EmptyOrAllWhitespace_ReturnsEmpty(string input)
    {
        Assert.Equal(string.Empty, Utils.TrimWhitespace(input).ToString());
    }

    [Theory]
    [InlineData("text", "text")]
    [InlineData("text with spaces", "text with spaces")]
    [InlineData("text\twith\ninternal whitespace", "text\twith\ninternal whitespace")]
    public void TrimWhitespace_TextWithoutOuterWhitespace_ReturnsSameText(
        string input,
        string expected)
    {
        Assert.Equal(expected, Utils.TrimWhitespace(input).ToString());
    }

    [Theory]
    [InlineData(" text", "text")]
    [InlineData("text ", "text")]
    [InlineData("  text  ", "text")]
    [InlineData("\t\ntext\r\n\t", "text")]
    public void TrimWhitespace_OuterWhitespace_ReturnsTrimmedText(
        string input,
        string expected)
    {
        Assert.Equal(expected, Utils.TrimWhitespace(input).ToString());
    }

    [Theory]
    [InlineData("  text with spaces  ", "text with spaces")]
    [InlineData("\ttext\twith\tspaces\t", "text\twith\tspaces")]
    public void TrimWhitespace_PreservesWhitespaceInsideText(
        string input,
        string expected)
    {
        Assert.Equal(expected, Utils.TrimWhitespace(input).ToString());
    }
}
