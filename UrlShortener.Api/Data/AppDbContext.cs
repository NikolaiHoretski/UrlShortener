using Microsoft.EntityFrameworkCore;

namespace UrlShortener.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ShortenedUrl> ShortenedUrls => Set<ShortenedUrl>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ShortenedUrl>();
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedNever();
        entity.Property(e => e.LongUrl).HasMaxLength(2048).IsRequired();
        entity.Property(e => e.Code).HasMaxLength(64).IsRequired();
        entity.HasIndex(e => e.Code).IsUnique();
        entity.Property(e => e.CreatedAt).IsRequired();
    }
}
