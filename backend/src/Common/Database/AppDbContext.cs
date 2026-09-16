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
            .HasMany(user => user.Websites);
    }

    public DbSet<UserModel> Users => Set<UserModel>();
    public DbSet<WebsiteModel> Websites => Set<WebsiteModel>();


}
