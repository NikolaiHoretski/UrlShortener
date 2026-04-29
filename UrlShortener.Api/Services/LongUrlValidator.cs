namespace UrlShortener.Api.Services;

public static class LongUrlValidator
{
    private const int MaxLength = 2048;

    public static bool TryValidate(string? longUrl, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(longUrl))
        {
            error = "Укажите полный URL.";
            return false;
        }

        if (longUrl.Length > MaxLength)
        {
            error = $"Полный URL не длиннее {MaxLength} символов.";
            return false;
        }

        if (!Uri.TryCreate(longUrl, UriKind.Absolute, out var uri) ||
            uri.Scheme is not ("http" or "https") ||
            string.IsNullOrEmpty(uri.Host))
        {
            error = "Полный URL должен быть абсолютным адресом http или https.";
            return false;
        }

        return true;
    }
}
