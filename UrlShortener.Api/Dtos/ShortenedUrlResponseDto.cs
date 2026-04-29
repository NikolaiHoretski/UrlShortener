namespace UrlShortener.Api.Dtos;

public sealed class ShortenedUrlResponseDto
{
    public required Guid Id { get; init; }

    public required string LongUrl { get; init; }

    public required string ShortUrl { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required int RedirectsCount { get; init; }
}
