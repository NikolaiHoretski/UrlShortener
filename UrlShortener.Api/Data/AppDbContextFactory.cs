using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UrlShortener.Api.Data;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var apiDir = FindApiProjectDirectory();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiDir)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString =
                "Server=127.0.0.1;Port=3306;Database=UrlShortenerDev;User=root;Password=dev;";
        }

        var versionString = configuration["Database:ServerVersion"];
        var serverVersion = string.IsNullOrWhiteSpace(versionString)
            ? new MariaDbServerVersion(new Version(10, 11, 0))
            : ServerVersion.Parse(versionString.Trim());

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseMySql(connectionString, serverVersion);
        return new AppDbContext(optionsBuilder.Options);
    }

    private static string FindApiProjectDirectory()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "UrlShortener.Api.csproj")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
