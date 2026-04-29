namespace UrlShortener.Api.Dtos;

public sealed record PagedUrlsResponseDto(
    IReadOnlyList<ShortenedUrlResponseDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
