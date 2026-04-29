namespace UrlShortener.Api.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public string Provider { get; set; } = "MariaDb";

    public string? ServerVersion { get; set; }
}
