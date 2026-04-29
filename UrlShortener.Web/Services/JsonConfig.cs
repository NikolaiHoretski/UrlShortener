using System.Text.Json;

namespace UrlShortener.Web.Services;

public static class JsonConfig
{
    public static JsonSerializerOptions ApiOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
    };
}
