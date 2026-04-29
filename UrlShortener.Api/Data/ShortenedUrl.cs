namespace UrlShortener.Api.Data;

public sealed class ShortenedUrl
{
    public Guid Id { get; set; }

    public required string LongUrl { get; set; }

    public required string Code { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public int RedirectsCount { get; set; }
}
