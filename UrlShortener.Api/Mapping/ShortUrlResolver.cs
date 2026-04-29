using AutoMapper;

using Microsoft.Extensions.Options;

using UrlShortener.Api.Data;
using UrlShortener.Api.Dtos;
using UrlShortener.Api.Options;

namespace UrlShortener.Api.Mapping;

public sealed class ShortUrlResolver(IOptions<UrlShortenerOptions> options)
    : IValueResolver<ShortenedUrl, ShortenedUrlResponseDto, string>
{
    public string Resolve(
        ShortenedUrl source,
        ShortenedUrlResponseDto? destination,
        string destMember,
        ResolutionContext context)
    {
        var baseUrl = options.Value.PublicBaseUrl.TrimEnd('/');
        return $"{baseUrl}/{source.Code}";
    }
}
