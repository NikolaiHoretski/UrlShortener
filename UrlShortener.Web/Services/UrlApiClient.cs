using System.Net;
using System.Net.Http.Json;

using UrlShortener.Web.Dtos;

namespace UrlShortener.Web.Services;

public sealed class UrlApiClient(HttpClient http)
{
    public async Task<PagedUrlsResponseDto?> ListAsync(
        int page,
        int pageSize,
        string sortBy,
        string sortOrder,
        CancellationToken cancellationToken = default)
    {
        var url =
            $"api/urls?page={page}&pageSize={pageSize}&sortBy={Uri.EscapeDataString(sortBy)}&sortOrder={Uri.EscapeDataString(sortOrder)}";
        using var response = await http.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<PagedUrlsResponseDto>(
            JsonConfig.ApiOptions,
            cancellationToken);
    }

    public async Task<(ShortenedUrlResponseDto? Created, string? Error)> CreateAsync(
        string longUrl,
        CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync(
            "api/urls",
            new CreateShortUrlRequestDto(longUrl),
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadFromJsonAsync<ShortenedUrlResponseDto>(
                JsonConfig.ApiOptions,
                cancellationToken);
            return (body, null);
        }

        var err = await response.Content.ReadFromJsonAsync<ErrorResponseDto>(JsonConfig.ApiOptions, cancellationToken);
        return (null, err?.Error ?? FallbackErrorMessage(response));
    }

    public async Task<(ShortenedUrlResponseDto? Updated, string? Error)> UpdateAsync(
        Guid id,
        string longUrl,
        CancellationToken cancellationToken = default)
    {
        var response = await http.PatchAsJsonAsync(
            $"api/urls/{id}",
            new UpdateLongUrlRequestDto(longUrl),
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadFromJsonAsync<ShortenedUrlResponseDto>(
                JsonConfig.ApiOptions,
                cancellationToken);
            return (body, null);
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return (null, "Запись не найдена.");
        }

        var err = await response.Content.ReadFromJsonAsync<ErrorResponseDto>(JsonConfig.ApiOptions, cancellationToken);
        return (null, err?.Error ?? FallbackErrorMessage(response));
    }

    public async Task<(bool Ok, string? Error)> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var response = await http.DeleteAsync($"api/urls/{id}", cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return (false, "Запись не найдена.");
        }

        return (false, FallbackErrorMessage(response));
    }

    private static string FallbackErrorMessage(HttpResponseMessage response) =>
        response.StatusCode switch
        {
            HttpStatusCode.BadRequest => "Некорректный запрос.",
            HttpStatusCode.Unauthorized => "Требуется авторизация.",
            HttpStatusCode.Forbidden => "Доступ запрещён.",
            HttpStatusCode.NotFound => "Ресурс не найден.",
            HttpStatusCode.InternalServerError => "Внутренняя ошибка сервера.",
            HttpStatusCode.ServiceUnavailable => "Сервис временно недоступен.",
            _ => $"Ошибка сервера (код {(int)response.StatusCode}).",
        };
}
