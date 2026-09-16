namespace a_alloc.Tests;

public class CommandLineTests
{
    public static IEnumerable<object[]> ValidArgumentOrders()
    {
        yield return new object[]
        {
            new[]
            {
                "--categories", "configs/categories.ini",
                "--symbols", "configs/symbols.ini",
                "--input", "reports/first.csv", "reports/second.csv"
            }
        };
        yield return new object[]
        {
            new[]
            {
                "--input", "reports/first.csv", "reports/second.csv",
                "--symbols", "configs/symbols.ini",
                "--categories", "configs/categories.ini"
            }
        };
        yield return new object[]
        {
            new[]
            {
                "--symbols", "configs/symbols.ini",
                "--categories", "configs/categories.ini",
                "--input", "reports/first.csv", "reports/second.csv"
            }
        };
    }

    public static IEnumerable<object[]> MissingRequiredOptions()
    {
        yield return new object[]
        {
            new[] { "--symbols", "symbols.ini", "--input", "report.csv" }
        };
        yield return new object[]
        {
            new[] { "--categories", "categories.ini", "--input", "report.csv" }
        };
        yield return new object[]
        {
            new[] { "--categories", "categories.ini", "--symbols", "symbols.ini" }
        };
        yield return new object[] { Array.Empty<string>() };
    }

    public static IEnumerable<object[]> OptionsMissingValues()
    {
        yield return new object[] { new[] { "--categories" } };
        yield return new object[]
        {
            new[] { "--categories", "categories.ini", "--symbols" }
        };
        yield return new object[]
        {
            new[] { "--categories", "categories.ini", "--symbols", "symbols.ini", "--input" }
        };
        yield return new object[]
        {
            new[] { "--categories", "--symbols", "symbols.ini", "--input", "report.csv" }
        };
        yield return new object[]
        {
            new[] { "--categories", "categories.ini", "--symbols", "--input", "report.csv" }
        };
    }

    public static IEnumerable<object[]> InvalidPaths()
    {
        yield return new object[]
        {
            new[] { "--categories", "\0", "--symbols", "symbols.ini", "--input", "report.csv" }
        };
        yield return new object[]
        {
            new[] { "--categories", "categories.ini", "--symbols", "\0", "--input", "report.csv" }
        };
        yield return new object[]
        {
            new[] { "--categories", "categories.ini", "--symbols", "symbols.ini", "--input", "\0" }
        };
    }

    [Theory]
    [MemberData(nameof(ValidArgumentOrders))]
    public void Constructor_RequiredOptionsInAnyOrder_IsValid(string[] args)
    {
        CommandLine commandLine = new(args);

        Assert.True(commandLine.IsValid());
        Assert.Equal("configs/categories.ini", commandLine.GetCategoriesConfig().ToString());
        Assert.Equal("configs/symbols.ini", commandLine.GetSymbolsConfig().ToString());
        Assert.Equal(2, commandLine.CountDataFiles());
        Assert.Equal("reports/first.csv", commandLine.GetDataFile(0).ToString());
        Assert.Equal("reports/second.csv", commandLine.GetDataFile(1).ToString());
    }

    [Fact]
    public void Constructor_NonexistentButSyntacticallyValidPaths_IsValid()
    {
        string[] args =
        {
            "--categories", "/not/a/real/directory/categories.ini",
            "--symbols", "files with spaces/symbols.ini",
            "--input", "missing/one.csv", "missing/two.csv"
        };

        CommandLine commandLine = new(args);

        Assert.True(commandLine.IsValid());
        Assert.Equal(2, commandLine.CountDataFiles());
    }

    [Theory]
    [MemberData(nameof(MissingRequiredOptions))]
    public void Constructor_MissingRequiredOption_IsInvalid(string[] args)
    {
        CommandLine commandLine = new(args);

        Assert.False(commandLine.IsValid());
    }

    [Theory]
    [MemberData(nameof(OptionsMissingValues))]
    public void Constructor_OptionWithoutAPath_IsInvalid(string[] args)
    {
        CommandLine commandLine = new(args);

        Assert.False(commandLine.IsValid());
        Assert.NotEmpty(commandLine.GetError().ToString());
    }

    [Theory]
    [MemberData(nameof(InvalidPaths))]
    public void Constructor_InvalidPath_IsInvalid(string[] args)
    {
        CommandLine commandLine = new(args);

        Assert.False(commandLine.IsValid());
        Assert.NotEmpty(commandLine.GetError().ToString());
    }

    [Fact]
    public void Constructor_DuplicateOption_IsInvalid()
    {
        string[] args =
        {
            "--categories", "first.ini",
            "--categories", "second.ini",
            "--symbols", "symbols.ini",
            "--input", "report.csv"
        };

        CommandLine commandLine = new(args);

        Assert.False(commandLine.IsValid());
    }

    [Fact(Skip = "Temporarily disabled while the parser is being revised")]
    public void Constructor_AdditionalCategoryArgument_IsInvalid()
    {
        string[] args =
        {
            "--categories", "categories.ini", "unexpected.ini",
            "--symbols", "symbols.ini",
            "--input", "report.csv"
        };

        CommandLine commandLine = new(args);

        Assert.False(commandLine.IsValid());
    }

    [Fact(Skip = "Temporarily disabled while the parser is being revised")]
    public void Constructor_AdditionalSymbolArgument_IsInvalid()
    {
        string[] args =
        {
            "--categories", "categories.ini",
            "--symbols", "symbols.ini", "unexpected.ini",
            "--input", "report.csv"
        };

        CommandLine commandLine = new(args);

        Assert.False(commandLine.IsValid());
    }

    [Fact(Skip = "Temporarily disabled while the parser is being revised")]
    public void Constructor_UnknownOption_IsInvalid()
    {
        string[] args =
        {
            "--categories", "categories.ini",
            "--symbols", "symbols.ini",
            "--input", "report.csv",
            "--unknown", "value"
        };

        CommandLine commandLine = new(args);

        Assert.False(commandLine.IsValid());
    }
}
