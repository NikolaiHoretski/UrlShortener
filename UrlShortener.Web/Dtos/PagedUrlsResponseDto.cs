using System.Text.Json.Serialization;

namespace UrlShortener.Web.Dtos;

public sealed record PagedUrlsResponseDto(
    [property: JsonPropertyName("items")] List<ShortenedUrlResponseDto> Items,
    [property: JsonPropertyName("totalCount")] int TotalCount,
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("pageSize")] int PageSize);
