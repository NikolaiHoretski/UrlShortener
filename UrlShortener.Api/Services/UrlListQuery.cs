using Microsoft.EntityFrameworkCore;

using UrlShortener.Api.Data;

namespace UrlShortener.Api.Services;

public static class UrlListQuery
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public static async Task<(IReadOnlyList<ShortenedUrl> Items, int TotalCount)> ListAsync(
        AppDbContext db,
        int page,
        int pageSize,
        string sortBy,
        string sortOrder,
        CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query = db.ShortenedUrls.AsNoTracking();
        var total = await query.CountAsync(cancellationToken);

        query = ApplySort(query, sortBy, sortOrder);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

  private static IQueryable<ShortenedUrl> ApplySort(
        IQueryable<ShortenedUrl> query,
        string sortBy,
        string sortOrder)
    {
        var descending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        if (MatchesSortBy(sortBy, nameof(ShortenedUrl.RedirectsCount)))
        {
            return descending
                ? query.OrderByDescending(x => x.RedirectsCount)
                : query.OrderBy(x => x.RedirectsCount);
        }

        if (MatchesSortBy(sortBy, nameof(ShortenedUrl.LongUrl)))
        {
            return descending
                ? query.OrderByDescending(x => x.LongUrl)
                : query.OrderBy(x => x.LongUrl);
        }

        if (MatchesSortBy(sortBy, nameof(ShortenedUrl.Code)))
        {
            return descending
                ? query.OrderByDescending(x => x.Code)
                : query.OrderBy(x => x.Code);
        }

        if (MatchesSortBy(sortBy, nameof(ShortenedUrl.CreatedAt)))
        {
            return descending
                ? query.OrderByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.CreatedAt);
        }

        return descending
            ? query.OrderByDescending(x => x.CreatedAt)
            : query.OrderBy(x => x.CreatedAt);
    }

    private static bool MatchesSortBy(string sortBy, string propertyName) =>
        string.Equals(sortBy, propertyName, StringComparison.OrdinalIgnoreCase);
}
