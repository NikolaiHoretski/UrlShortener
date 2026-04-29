using System.Security.Cryptography;

using Microsoft.Extensions.Options;

using UrlShortener.Api.Options;

namespace UrlShortener.Api.Services;

public sealed class ShortCodeGenerator(IOptions<UrlShortenerOptions> options) : IShortCodeGenerator
{
    private static readonly string AvailableCharacters = string.Create(62, 0, static (chars, _) =>
    {
        var i = 0;
        for (var c = 'a'; c <= 'z'; c++)
        {
            chars[i++] = c;
        }

        for (var c = 'A'; c <= 'Z'; c++)
        {
            chars[i++] = c;
        }

        for (var c = '0'; c <= '9'; c++)
        {
            chars[i++] = c;
        }
    });

    public string Generate()
    {
        var length = options.Value.CodeLength;
        if (length is < 4 or > 64)
        {
            throw new InvalidOperationException("UrlShortener:CodeLength должен быть от 4 до 64.");
        }

        return string.Create(length, 0, (chars, _) =>
        {
            var buffer = new byte[length];
            RandomNumberGenerator.Fill(buffer);
            for (var i = 0; i < length; i++)
            {
                chars[i] = AvailableCharacters[buffer[i] % AvailableCharacters.Length];
            }
        });
    }
}
