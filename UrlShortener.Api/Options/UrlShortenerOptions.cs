namespace UrlShortener.Api.Options;

public sealed class UrlShortenerOptions
{
    public const string SectionName = "UrlShortener";

    public string PublicBaseUrl { get; set; } = string.Empty;

    public int CodeLength { get; set; } = 8;
}
