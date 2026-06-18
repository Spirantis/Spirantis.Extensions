namespace Spirantis.Extensions.SystemTests;

/// <summary>
/// Tests for the <c>string</c> and <c>ReadOnlySpan&lt;char&gt;</c> trimming
/// extensions declared in <c>StringExtensions</c>.
/// </summary>
[Trait("Category", "Unit")]
public sealed class StringExtensionsTests
{
    [Theory]
    [InlineData("hello.txt", ".txt", StringComparison.Ordinal, "hello")]
    [InlineData("hello.txt.txt", ".txt", StringComparison.Ordinal, "hello")]
    [InlineData("hello", ".txt", StringComparison.Ordinal, "hello")]
    [InlineData("hello", "", StringComparison.Ordinal, "hello")]
    [InlineData("FILE.TXT", ".txt", StringComparison.OrdinalIgnoreCase, "FILE")]
    [InlineData("FILE.TXT", ".txt", StringComparison.Ordinal, "FILE.TXT")]
    [InlineData("abab", "ab", StringComparison.Ordinal, "")]
    public void TrimEnd_OnString_RemovesRepeatedTrailingValue(
        string source,
        string value,
        StringComparison comparison,
        string expected
    )
    {
        Assert.Equal(expected, source.TrimEnd(value, comparison));
    }

    [Theory]
    [InlineData("www.site.com", "www.", StringComparison.Ordinal, "site.com")]
    [InlineData("xxxabc", "x", StringComparison.Ordinal, "abc")]
    [InlineData("abc", "x", StringComparison.Ordinal, "abc")]
    [InlineData("abc", "", StringComparison.Ordinal, "abc")]
    [InlineData("WWW.site", "www.", StringComparison.OrdinalIgnoreCase, "site")]
    [InlineData("abab", "ab", StringComparison.Ordinal, "")]
    public void TrimStart_OnString_RemovesRepeatedLeadingValue(
        string source,
        string value,
        StringComparison comparison,
        string expected
    )
    {
        Assert.Equal(expected, source.TrimStart(value, comparison));
    }

    [Fact]
    public void TrimEnd_OnNullString_Throws()
    {
        string source = null!;
        Assert.Throws<ArgumentNullException>(() =>
            _ = source.TrimEnd(".txt", StringComparison.Ordinal)
        );
    }

    [Fact]
    public void TrimStart_OnNullString_Throws()
    {
        string source = null!;
        Assert.Throws<ArgumentNullException>(() =>
            _ = source.TrimStart("x", StringComparison.Ordinal)
        );
    }

    [Theory]
    [InlineData("hello.txt.txt", ".txt", StringComparison.Ordinal, "hello")]
    [InlineData("hello", ".txt", StringComparison.Ordinal, "hello")]
    [InlineData("hello", "", StringComparison.Ordinal, "hello")]
    [InlineData("abab", "ab", StringComparison.Ordinal, "")]
    public void TrimEnd_OnSpan_ReturnsTrimmedSlice(
        string source,
        string value,
        StringComparison comparison,
        string expected
    )
    {
        ReadOnlySpan<char> result = source.AsSpan().TrimEnd(value, comparison);
        Assert.Equal(expected, result.ToString());
    }

    [Theory]
    [InlineData("xxxabc", "x", StringComparison.Ordinal, "abc")]
    [InlineData("abc", "x", StringComparison.Ordinal, "abc")]
    [InlineData("abc", "", StringComparison.Ordinal, "abc")]
    [InlineData("abab", "ab", StringComparison.Ordinal, "")]
    public void TrimStart_OnSpan_ReturnsTrimmedSlice(
        string source,
        string value,
        StringComparison comparison,
        string expected
    )
    {
        ReadOnlySpan<char> result = source.AsSpan().TrimStart(value, comparison);
        Assert.Equal(expected, result.ToString());
    }
}
