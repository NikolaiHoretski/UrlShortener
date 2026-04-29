using UrlShortener.Api.Services;

namespace UrlShortener.Tests;

public sealed class LongUrlValidatorTests
{
    [Theory]
    [InlineData("https://example.com/path", true)]
    [InlineData("http://localhost:5000/x", true)]
    [InlineData("ftp://example.com", false)]
    [InlineData("not-a-url", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    public void TryValidate_ReturnsExpectedOk_ForInput(string url, bool expectedOk)
    {
        var ok = LongUrlValidator.TryValidate(url, out var error);
        Assert.Equal(expectedOk, ok);
        if (!expectedOk)
        {
            Assert.False(string.IsNullOrEmpty(error));
        }
    }

    [Fact]
    public void TryValidate_ReturnsFalse_WhenUrlExceedsMaxLength()
    {
        var url = "https://example.com/" + new string('a', 3000);
        var ok = LongUrlValidator.TryValidate(url, out _);
        Assert.False(ok);
    }
}
