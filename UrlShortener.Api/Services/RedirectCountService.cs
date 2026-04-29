using Microsoft.EntityFrameworkCore;

using UrlShortener.Api.Data;

namespace UrlShortener.Api.Services;

public sealed class RedirectCountService(AppDbContext db) : IRedirectCountService
{
   public async Task IncrementByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var row = await db.ShortenedUrls.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (row is null)
            return;

        row.RedirectsCount++;
        await db.SaveChangesAsync(cancellationToken);
    }
}
