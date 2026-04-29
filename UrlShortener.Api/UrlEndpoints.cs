using AutoMapper;

using Microsoft.EntityFrameworkCore;

using UrlShortener.Api.Data;
using UrlShortener.Api.Dtos;
using UrlShortener.Api.Services;

namespace UrlShortener.Api;

public static class UrlEndpoints
{
    public static void MapUrlEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/urls").WithTags("urls");

        group.MapGet("/", ListUrls);
        group.MapPost("/", CreateUrl);
        group.MapPatch("/{id:guid}", UpdateUrl);
        group.MapDelete("/{id:guid}", DeleteUrl);

        app.MapGet(@"/{code:regex(^[a-zA-Z0-9]{{4,64}}$)}", RedirectByCode).WithTags("redirect");
    }

    private static async Task<IResult> ListUrls(
        AppDbContext db,
        IMapper mapper,
        int page = 1,
        int pageSize = UrlListQuery.DefaultPageSize,
        string sortBy = "createdAt",
        string sortOrder = "desc",
        CancellationToken cancellationToken = default)
    {
        var (items, total) = await UrlListQuery.ListAsync(db, page, pageSize, sortBy, sortOrder, cancellationToken);
        var dtos = mapper.Map<IReadOnlyList<ShortenedUrlResponseDto>>(items);
        return Results.Ok(new PagedUrlsResponseDto(dtos, total, page, pageSize));
    }

    private static async Task<IResult> CreateUrl(
        CreateShortUrlRequestDto request,
        AppDbContext db,
        IMapper mapper,
        IShortCodeGenerator codeGenerator,
        CancellationToken cancellationToken)
    {
        if (!LongUrlValidator.TryValidate(request.LongUrl, out var error))
            return Results.BadRequest(new { error });

        const int maxAttempts = 10;
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var code = codeGenerator.Generate();
            var entity = new ShortenedUrl
            {
                Id = Guid.CreateVersion7(),
                LongUrl = request.LongUrl.Trim(),
                Code = code,
                CreatedAt = DateTimeOffset.UtcNow,
                RedirectsCount = 0,
            };

            db.ShortenedUrls.Add(entity);
            try
            {
                await db.SaveChangesAsync(cancellationToken);
                return Results.Created($"/api/urls/{entity.Id}", mapper.Map<ShortenedUrlResponseDto>(entity));
            }
            catch (DbUpdateException)
            {
                db.Entry(entity).State = EntityState.Detached;
            }
        }

        return Results.Problem(
            title: "Не удалось выделить уникальный короткий код.",
            detail: "Повторите запрос позже. Если ошибка сохраняется, обратитесь к администратору.",
            statusCode: StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> UpdateUrl(
        Guid id,
        UpdateLongUrlRequestDto request,
        AppDbContext db,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        if (!LongUrlValidator.TryValidate(request.LongUrl, out var error))
            return Results.BadRequest(new { error });

        var entity = await db.ShortenedUrls.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
            return Results.NotFound();

        entity.LongUrl = request.LongUrl.Trim();
        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(mapper.Map<ShortenedUrlResponseDto>(entity));
    }

    private static async Task<IResult> DeleteUrl(
        Guid id,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        var entity = await db.ShortenedUrls.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Results.NotFound();
        }

        db.ShortenedUrls.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> RedirectByCode(
        string code,
        HttpContext http,
        AppDbContext db,
        IServiceScopeFactory scopeFactory,
        CancellationToken cancellationToken)
    {
        var entity = await db.ShortenedUrls
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == code, cancellationToken);

        if (entity is null)
            return Results.NotFound();

        var id = entity.Id;
        http.Response.OnCompleted(async () =>
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var counter = scope.ServiceProvider.GetRequiredService<IRedirectCountService>();
            try
            {
                await counter.IncrementByIdAsync(id, CancellationToken.None);
            }
            catch
            {
                // Метрика best-effort: сбой счётчика не должен ломать завершение ответа.
            }
        });

        return Results.Redirect(entity.LongUrl);
    }
}
