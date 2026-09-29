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
        // User -> Website
        modelBuilder.Entity<UserModel>()
            .HasMany(user => user.Websites)
            .WithOne(website => website.User)
            .HasForeignKey(user => user.UserId);

        // Website -> Links
        modelBuilder.Entity<WebsiteModel>()
            .HasMany(website => website.WebsiteLinks)
            .WithOne(link => link.Website)
            .HasForeignKey(website => website.WebsiteId);

        // Website -> Credential
        modelBuilder.Entity<WebsiteModel>()
            .HasMany(website => website.WebsiteCredentials)
            .WithOne(credential => credential.Website)
            .HasForeignKey(website => website.WebsiteId);
    }

    public DbSet<UserModel> Users => Set<UserModel>();
    public DbSet<WebsiteModel> Websites => Set<WebsiteModel>();
    public DbSet<WebsiteLinkModel> WebsiteLinks => Set<WebsiteLinkModel>();
    public DbSet<WebsiteCredentialModel> WebsiteCredentials => Set<WebsiteCredentialModel>();
}
