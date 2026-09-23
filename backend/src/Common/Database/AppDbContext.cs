using Microsoft.EntityFrameworkCore;
using PasswordVault.Common.Models;

namespace PasswordVault.Common.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserModel>()
            .HasMany(user => user.Websites)
            .WithOne(website => website.User)
            .HasForeignKey(website => website.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WebsiteModel>()
            .HasMany(website => website.Urls)
            .WithOne(url => url.Website)
            .HasForeignKey(url => url.WebsiteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WebsiteModel>()
            .HasMany(website => website.Credentials)
            .WithOne(credential => credential.Website)
            .HasForeignKey(credential => credential.WebsiteId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public DbSet<UserModel> Users => Set<UserModel>();
    public DbSet<WebsiteModel> Websites => Set<WebsiteModel>();
    public DbSet<WebsiteUrlModel> WebsiteUrls => Set<WebsiteUrlModel>();
    public DbSet<WebsiteCredentialModel> WebsiteCredentials => Set<WebsiteCredentialModel>();
}
