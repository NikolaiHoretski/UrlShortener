using System.Text.Json.Serialization;

namespace UrlShortener.Web.Dtos;

public sealed record ErrorResponseDto(
    [property: JsonPropertyName("error")] string? Error);
