using System.Text.Json.Serialization;

namespace UrlShortener.Web.Dtos;

public sealed record CreateShortUrlRequestDto(
    [property: JsonPropertyName("longUrl")] string LongUrl);
