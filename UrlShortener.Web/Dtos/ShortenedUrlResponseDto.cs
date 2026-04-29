using System.Text.Json.Serialization;

namespace UrlShortener.Web.Dtos;

public sealed record ShortenedUrlResponseDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("longUrl")] string LongUrl,
    [property: JsonPropertyName("shortUrl")] string ShortUrl,
    [property: JsonPropertyName("createdAt")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("redirectsCount")] int RedirectsCount);
