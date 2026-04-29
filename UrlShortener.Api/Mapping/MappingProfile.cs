using AutoMapper;

using UrlShortener.Api.Data;
using UrlShortener.Api.Dtos;

namespace UrlShortener.Api.Mapping;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ShortenedUrl, ShortenedUrlResponseDto>()
            .ForMember(d => d.ShortUrl, o => o.MapFrom<ShortUrlResolver>());
    }
}
