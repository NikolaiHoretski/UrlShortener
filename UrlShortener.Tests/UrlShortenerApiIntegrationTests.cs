using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;

using UrlShortener.Api.Data;
using UrlShortener.Api.Dtos;

namespace UrlShortener.Tests;

[Collection("Api integration")]
public sealed class UrlShortenerApiIntegrationTests(ApiWebApplicationFactory factory)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly HttpClient _client = factory.CreateClient();
    private readonly IServiceProvider _services = factory.Services;

    [Fact]
    public async Task CreateMany_PostsWithRandomLongUrls_AllReturnCreatedAndUniqueCodes()
    {
        await ClearAllUrlsAsync();
        var count = Random.Shared.Next(10, 31);
        var codes = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < count; i++)
        {
            var longUrl = $"https://example.com/{Guid.NewGuid():N}/{Random.Shared.Next()}";
            using var response = await _client.PostAsJsonAsync("/api/urls", new CreateShortUrlRequestDto(longUrl), JsonOptions);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var dto = await response.Content.ReadFromJsonAsync<ShortenedUrlResponseDto>(JsonOptions);
            Assert.NotNull(dto);
            Assert.Equal(longUrl, dto.LongUrl);
            var code = dto.ShortUrl.TrimEnd('/').Split('/').Last();
            Assert.True(codes.Add(code), $"Код должен быть уникальным: {code}");
        }

        Assert.Equal(count, codes.Count);
    }

    [Fact]
    public async Task ListUrls_AfterRandomSeed_ReturnsMatchingTotalCount()
    {
        await ClearAllUrlsAsync();
        var count = Random.Shared.Next(10, 31);
        await SeedRandomUrlsAsync(count);
        using var response = await _client.GetAsync("/api/urls?page=1&pageSize=100&sortBy=createdAt&sortOrder=desc");
        response.EnsureSuccessStatusCode();
        var page = await response.Content.ReadFromJsonAsync<PagedUrlsResponseDto>(JsonOptions);
        Assert.NotNull(page);
        Assert.Equal(count, page.TotalCount);
        Assert.Equal(count, page.Items.Count);
    }

    [Fact]
    public async Task ListUrls_SortByLongUrlAscending_MatchesExpectedOrder()
    {
        await ClearAllUrlsAsync();
        var count = Random.Shared.Next(10, 31);
        var longUrls = new List<string>();
        for (var i = 0; i < count; i++)
        {
            longUrls.Add($"https://example.com/z/{Guid.NewGuid():N}/{Random.Shared.Next(1000, 9999)}");
        }

        foreach (var url in longUrls)
        {
            using var post = await _client.PostAsJsonAsync("/api/urls", new CreateShortUrlRequestDto(url), JsonOptions);
            post.EnsureSuccessStatusCode();
        }

        var expected = longUrls.OrderBy(u => u, StringComparer.Ordinal).ToList();
        using var response = await _client.GetAsync("/api/urls?page=1&pageSize=100&sortBy=longUrl&sortOrder=asc");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<PagedUrlsResponseDto>(JsonOptions);
        Assert.NotNull(body);
        var actual = body.Items.Select(x => x.LongUrl).ToList();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task ListUrls_SortByCodeAscending_IsLexicographicallySorted()
    {
        await ClearAllUrlsAsync();
        var count = Random.Shared.Next(10, 31);
        await SeedRandomUrlsAsync(count);
        using var response = await _client.GetAsync("/api/urls?page=1&pageSize=100&sortBy=code&sortOrder=asc");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<PagedUrlsResponseDto>(JsonOptions);
        Assert.NotNull(body);
        var codes = body.Items
            .Select(x => x.ShortUrl.TrimEnd('/').Split('/').Last())
            .ToList();
        var sorted = codes.OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
        Assert.Equal(sorted, codes);
    }

    [Fact]
    public async Task DeleteMany_RandomSubset_RemainingListMatchesExpectation()
    {
        await ClearAllUrlsAsync();
        var count = Random.Shared.Next(15, 36);
        var ids = await SeedRandomUrlsAsync(count);
        var toDelete = ids.OrderBy(_ => Random.Shared.Next()).Take(Random.Shared.Next(10, Math.Min(20, count))).ToList();
        foreach (var id in toDelete)
        {
            using var del = await _client.DeleteAsync($"/api/urls/{id}");
            Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);
        }

        var remaining = ids.Except(toDelete).ToHashSet();
        using var list = await _client.GetAsync("/api/urls?page=1&pageSize=100&sortBy=createdAt&sortOrder=asc");
        list.EnsureSuccessStatusCode();
        var body = await list.Content.ReadFromJsonAsync<PagedUrlsResponseDto>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(remaining.Count, body.TotalCount);
        Assert.All(body.Items, x => Assert.Contains(x.Id, remaining));
    }

    private async Task ClearAllUrlsAsync()
    {
        await using var scope = _services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.ShortenedUrls.RemoveRange(db.ShortenedUrls);
        await db.SaveChangesAsync();
    }

    private async Task<List<Guid>> SeedRandomUrlsAsync(int count)
    {
        var ids = new List<Guid>();
        for (var i = 0; i < count; i++)
        {
            var longUrl = $"https://example.com/seed/{Guid.NewGuid():N}/{Random.Shared.Next()}";
            using var response = await _client.PostAsJsonAsync("/api/urls", new CreateShortUrlRequestDto(longUrl), JsonOptions);
            response.EnsureSuccessStatusCode();
            var dto = await response.Content.ReadFromJsonAsync<ShortenedUrlResponseDto>(JsonOptions);
            Assert.NotNull(dto);
            ids.Add(dto.Id);
        }

        return ids;
    }
}
