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
            .Property(user => user.Uuid)
            .HasConversion<string>()
            .HasColumnType("varchar");

        modelBuilder.Entity<WebsiteModel>()
            .Property(website => website.Uuid)
            .HasConversion<string>()
            .HasColumnType("varchar");

        modelBuilder.Entity<UserModel>()
            .HasMany(user => user.Websites)
            .WithOne(website => website.User)
            .HasForeignKey(website => website.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WebsiteModel>()
            .HasMany(website => website.Links)
            .WithOne(link => link.Website)
            .HasForeignKey(link => link.WebsiteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WebsiteModel>()
            .HasMany(website => website.Credentials)
            .WithOne(credential => credential.Website)
            .HasForeignKey(credential => credential.WebsiteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserWebsiteLinkModel>()
            .Property(link => link.Uuid)
            .HasDefaultValueSql("gen_random_uuid()::text");

        modelBuilder.Entity<UserWebsiteCredentialModel>()
            .Property(credential => credential.Uuid)
            .HasDefaultValueSql("gen_random_uuid()::text");
    }

    public DbSet<UserModel> Users => Set<UserModel>();
    public DbSet<WebsiteModel> Websites => Set<WebsiteModel>();
    public DbSet<UserWebsiteLinkModel> WebsiteLinks => Set<UserWebsiteLinkModel>();
    public DbSet<UserWebsiteCredentialModel> WebsiteCredentials => Set<UserWebsiteCredentialModel>();
}
