using Microsoft.Extensions.Options;

using UrlShortener.Api.Options;
using UrlShortener.Api.Services;

namespace UrlShortener.Tests;

public sealed class ShortCodeGeneratorTests
{
    [Fact]
    public void Generate_ReturnsCodeWithConfiguredLength()
    {
        var generator = CreateGenerator(codeLength: 12);
        var code = generator.Generate();
        Assert.Equal(12, code.Length);
    }

    [Fact]
    public void Generate_UsesOnlyEnglishLettersAndDigits()
    {
        var generator = CreateGenerator(codeLength: 32);
        var code = generator.Generate();
        Assert.All(code, c => Assert.True(IsEnglishLetterOrDigit(c), $"Недопустимый символ: {c}"));
    }

    [Fact]
    public void Generate_ProducesMostlyUniqueValues()
    {
        var generator = CreateGenerator(codeLength: 8);
        var set = new HashSet<string>();
        for (var i = 0; i < 200; i++)
        {
            set.Add(generator.Generate());
        }

        Assert.True(set.Count > 190, "При такой длине кодов совпадения должны быть крайне редки.");
    }

    [Theory]
    [InlineData(3)]
    [InlineData(65)]
    public void Generate_ThrowsWhenCodeLengthOutOfRange(int length)
    {
        var generator = CreateGenerator(length);
        Assert.Throws<InvalidOperationException>(() => generator.Generate());
    }

    private static ShortCodeGenerator CreateGenerator(int codeLength)
    {
        var options = Options.Create(new UrlShortenerOptions { CodeLength = codeLength });
        return new ShortCodeGenerator(options);
    }

    private static bool IsEnglishLetterOrDigit(char c) =>
        c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or (>= '0' and <= '9');
}
