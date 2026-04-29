using Microsoft.EntityFrameworkCore;

using Scalar.AspNetCore;

using UrlShortener.Api;
using UrlShortener.Api.Data;
using UrlShortener.Api.Mapping;
using UrlShortener.Api.Options;
using UrlShortener.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<UrlShortenerOptions>(
    builder.Configuration.GetSection(UrlShortenerOptions.SectionName));
builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection(DatabaseOptions.SectionName));

builder.Services.AddOpenApi();
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
builder.Services.AddSingleton<IShortCodeGenerator, ShortCodeGenerator>();
builder.Services.AddScoped<IRedirectCountService, RedirectCountService>();

var databaseOptions = builder.Configuration
    .GetSection(DatabaseOptions.SectionName)
    .Get<DatabaseOptions>() ?? new DatabaseOptions();

if (string.Equals(databaseOptions.Provider, "InMemory", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDbContext<AppDbContext>(o =>
        o.UseInMemoryDatabase("UrlShortener"));
}
else if (IsMariaDbProvider(databaseOptions.Provider))
{
    var connectionString = builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException(
            "Для MariaDb задайте ConnectionStrings:Default (или переменную ConnectionStrings__Default).");
    var serverVersion = ResolveServerVersion(databaseOptions.ServerVersion);
    builder.Services.AddDbContext<AppDbContext>(o =>
        o.UseMySql(connectionString, serverVersion, mysql =>
            mysql.EnableRetryOnFailure()));
}
else
{
    throw new InvalidOperationException(
        $"Неизвестный Database:Provider «{databaseOptions.Provider}». Допустимо: InMemory, MariaDb.");
}

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

if (!string.Equals(databaseOptions.Provider, "InMemory", StringComparison.OrdinalIgnoreCase))
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseCors();
app.MapOpenApi();
app.MapScalarApiReference();

app.MapUrlEndpoints();

app.Run();

public partial class Program
{
    private static bool IsMariaDbProvider(string provider) =>
        string.Equals(provider, "MariaDb", StringComparison.OrdinalIgnoreCase)
        || string.Equals(provider, "MySql", StringComparison.OrdinalIgnoreCase);

    private static ServerVersion ResolveServerVersion(string? configured)
    {
        if (string.IsNullOrWhiteSpace(configured))
        {
            return new MariaDbServerVersion(new Version(10, 11, 0));
        }

        return ServerVersion.Parse(configured.Trim());
    }
}
