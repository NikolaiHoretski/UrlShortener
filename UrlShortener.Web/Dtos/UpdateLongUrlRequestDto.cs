using System.Text.Json.Serialization;

namespace UrlShortener.Web.Dtos;

public sealed record UpdateLongUrlRequestDto(
    [property: JsonPropertyName("longUrl")] string LongUrl);
