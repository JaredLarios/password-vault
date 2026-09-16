using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using PasswordVault.Common.Database;
using PasswordVault.Common.Models;

namespace PasswordVaultAPI.Tests;

public class ModelsAndDatabaseTests
{
    [Fact]
    public void Models_InitializeExpectedDefaults()
    {
        var user = new UserModel();
        var website = new WebsiteModel();

        user.Id = 1;
        website.Id = 2;
        website.UserId = user.Id;

        Assert.False(user.isActive);
        Assert.Equal(1, user.Id);
        Assert.True(user.is_temporal);
        Assert.NotEqual(Guid.Empty, user.Uuid);
        Assert.NotNull(user.Websites);
        Assert.Equal(string.Empty, user.UsernameFer);
        Assert.Equal(string.Empty, user.Password);
        Assert.NotEqual(Guid.Empty, website.Uuid);
        Assert.Equal(2, website.Id);
        Assert.Equal(string.Empty, website.WebsiteName);
        Assert.Equal(1, website.UserId);
        Assert.Null(website.UpdatedAt);
        Assert.Null(website.DeletedAt);
        Assert.InRange(user.CreatedAt, DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(5));
    }

    [Fact]
    public void AppDbContext_ConfiguresTablesAndUserWebsiteRelationship()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=test")
            .Options;
        using var context = new AppDbContext(options);

        var userEntity = context.Model.FindEntityType(typeof(UserModel));
        var websiteEntity = context.Model.FindEntityType(typeof(WebsiteModel));

        Assert.Equal("sys_user", userEntity?.GetTableName());
        Assert.Equal("public", userEntity?.GetSchema());
        Assert.Equal("user_website", websiteEntity?.GetTableName());
        Assert.NotNull(userEntity?.FindNavigation(nameof(UserModel.Websites)));
        Assert.Equal(nameof(WebsiteModel.User), websiteEntity?.FindNavigation(nameof(WebsiteModel.User))?.Name);
        Assert.Equal(nameof(WebsiteModel.UserId), websiteEntity?.GetForeignKeys().Single().Properties.Single().Name);
        Assert.NotNull(context.Users);
        Assert.NotNull(context.Websites);
    }

    [Fact]
    public void AuthDto_EnforcesRequiredEmailAndLengthRules()
    {
        var invalid = new PasswordVault.Auth.AuthDTO
        {
            Username = "not-an-email",
            Password = string.Empty
        };
        var context = new ValidationContext(invalid);
        var errors = new List<ValidationResult>();

        bool valid = Validator.TryValidateObject(invalid, context, errors, validateAllProperties: true);

        Assert.False(valid);
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(invalid.Username)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(invalid.Password)));
    }
}
