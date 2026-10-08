using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PasswordVault.Auth;
using PasswordVault.Common.Database;
using PasswordVault.Common.Models;
using PasswordVault.Common.Repositories;
using PasswordVault.Users;
using PasswordVault.Website;

namespace PasswordVaultAPI.Tests;

public class FabianFeatureTests
{
    [Fact]
    public async Task RegisterUser_CreatesUserWithHashedPasswordAndUniqueUsername()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);
        var service = new UserService(context, new FernetRepository("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA="), new Argon2Repository(), new Sha256Repository());

        var result = await service.CreateUserAsync(new NewUserDTO
        {
            Name = "Fabian",
            LastName = "Betancourt",
            Username = "fabian@example.com",
            Password = "Password123!"
        });

        Assert.NotNull(result);
        Assert.Equal("User created successfully", result.Message);
        Assert.NotEqual(Guid.Empty, result.UserId);
        Assert.Equal(1, await context.Users.CountAsync());
    }

    [Fact]
    public async Task RegisterUser_RejectsDuplicateUsername()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);
        var service = new UserService(context, new FernetRepository("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA="), new Argon2Repository(), new Sha256Repository());

        await service.CreateUserAsync(new NewUserDTO
        {
            Name = "Fabian",
            LastName = "Betancourt",
            Username = "fabian@example.com",
            Password = "Password123!"
        });

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateUserAsync(new NewUserDTO
            {
                Name = "Fabian",
                LastName = "Betancourt",
                Username = "fabian@example.com",
                Password = "Password123!"
            }));

        Assert.Equal("A user with that username already exists.", exception.Message);
    }

    [Fact]
    public async Task GetProfile_ReturnsCurrentUserWithoutPassword()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);
        var crypto = new FernetRepository("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=");
        var user = new UserModel
        {
            UsernameFer = crypto.GetEncryptedText("fabian@example.com"),
            UsernameSha = "sha256",
            NameFer = crypto.GetEncryptedText("Fabian"),
            LastNameFer = crypto.GetEncryptedText("Betancourt"),
            Password = "hashed-password",
            isActive = true
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new UserService(context, crypto, new Argon2Repository(), new Sha256Repository());
        var result = await service.GetCurrentUserAsync(user.Uuid);

        Assert.NotNull(result);
        Assert.Equal("fabian@example.com", result!.Username);
        Assert.Equal("Fabian", result.Name);
        Assert.Equal("Betancourt", result.LastName);
    }

    [Fact]
    public async Task AuthRequiredMiddleware_RejectsMissingToken()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);
        var service = new UserService(context, new FernetRepository("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA="), new Argon2Repository(), new Sha256Repository());
        var controller = new UserController(service);

        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var action = await controller.GetProfile();

        Assert.IsType<UnauthorizedResult>(action.Result);
    }

    [Fact]
    public async Task AuthRequiredMiddleware_AllowsAuthenticatedRequest()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);
        var crypto = new FernetRepository("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=");
        var user = new UserModel
        {
            UsernameFer = crypto.GetEncryptedText("fabian@example.com"),
            UsernameSha = "sha256",
            NameFer = crypto.GetEncryptedText("Fabian"),
            LastNameFer = crypto.GetEncryptedText("Betancourt"),
            Password = "hashed-password",
            isActive = true
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new UserService(context, crypto, new Argon2Repository(), new Sha256Repository());
        var controller = new UserController(service);

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, user.Uuid.ToString())],
            "Test"));
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var action = await controller.GetProfile();

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var payload = Assert.IsType<UserProfileResponse>(ok.Value);
        Assert.Equal("fabian@example.com", payload.Username);
    }

    [Fact]
    public async Task WebsiteService_GetWebsites_ReturnsUrlsAndCredentialsForCurrentUser()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);
        var crypto = new FernetRepository("MDEyMzQ1Njc4OWFiY2RlZjAxMjM0NTY3ODlhYmNkZWY=");
        var user = new UserModel
        {
            Uuid = Guid.NewGuid(),
            UsernameFer = crypto.GetEncryptedText("user@example.com"),
            UsernameSha = "sha256",
            Password = "hashed-password",
            isActive = true
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var website = new WebsiteModel
        {
            Uuid = Guid.NewGuid(),
            UserId = user.Id,
            WebsiteName = "Facebook"
        };
        context.Websites.Add(website);
        context.WebsiteLinks.Add(new WebsiteLinkModel
        {
            WebsiteId = website.Id,
            WebsiteUrl = "https://www.facebook.com"
        });
        context.WebsiteCredentials.Add(new WebsiteCredentialModel
        {
            WebsiteId = website.Id,
            WebsiteUsernameFer = crypto.GetEncryptedText("user@example.com"),
            WebsiteUsernameSha = new Sha256Repository().GetHash("user@example.com"),
            WebsitePasswordFer = crypto.GetEncryptedText("secret-pass"),
            WebsitePasswordSha = new Sha1Repository().GetHash("secret-pass")
        });
        await context.SaveChangesAsync();

        var service = new WebsiteService(context, crypto, new Sha256Repository(), new Sha1Repository());

        var result = await service.GetWebsitesAsync(user.Uuid, website.Uuid, null);

        var item = Assert.Single(result);
        Assert.Equal("Facebook", item.WebsiteName);
        Assert.Contains("https://www.facebook.com", item.Urls);
        var credential = Assert.Single(item.Credentials);
        Assert.Equal("user@example.com", credential.WebsiteUsername);
        Assert.Equal("secret-pass", credential.WebsitePassword);
    }

    [Fact]
    public async Task WebsiteService_UpdateWebsite_UpdatesWebsiteNameAndCredentialValues()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        using var context = new AppDbContext(options);
        var crypto = new FernetRepository("MDEyMzQ1Njc4OWFiY2RlZjAxMjM0NTY3ODlhYmNkZWY=");
        var user = new UserModel
        {
            Uuid = Guid.NewGuid(),
            UsernameFer = crypto.GetEncryptedText("user@example.com"),
            UsernameSha = "sha256",
            Password = "hashed-password",
            isActive = true
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var website = new WebsiteModel
        {
            Uuid = Guid.NewGuid(),
            UserId = user.Id,
            WebsiteName = "Old Name"
        };
        context.Websites.Add(website);
        context.WebsiteLinks.Add(new WebsiteLinkModel
        {
            WebsiteId = website.Id,
            WebsiteUrl = "https://old.example.com"
        });
        var credential = new WebsiteCredentialModel
        {
            WebsiteId = website.Id,
            WebsiteUsernameFer = crypto.GetEncryptedText("old@example.com"),
            WebsiteUsernameSha = new Sha256Repository().GetHash("old@example.com"),
            WebsitePasswordFer = crypto.GetEncryptedText("old-pass"),
            WebsitePasswordSha = new Sha1Repository().GetHash("old-pass")
        };
        context.WebsiteCredentials.Add(credential);
        await context.SaveChangesAsync();

        var service = new WebsiteService(context, crypto, new Sha256Repository(), new Sha1Repository());

        var response = await service.UpdateWebsiteAsync(
            user.Uuid,
            website.Uuid,
            new UpdateWebsiteDTO
            {
                WebsiteName = "New Name",
                WebsiteUrl = "https://new.example.com",
                WebsiteUsername = "new@example.com",
                WebsitePassword = "new-pass"
            });

        Assert.Equal("Website credentials updated successfully.", response.Message);

        var updated = await context.Websites.Include(x => x.WebsiteLinks).Include(x => x.WebsiteCredentials)
            .SingleAsync(x => x.Uuid == website.Uuid);
        Assert.Equal("New Name", updated.WebsiteName);
        Assert.Contains(updated.WebsiteLinks, x => x.WebsiteUrl == "https://new.example.com");
        Assert.Contains(updated.WebsiteCredentials, x =>
            crypto.GetDecryptedText(x.WebsiteUsernameFer) == "new@example.com" &&
            x.WebsiteUsernameSha == new Sha256Repository().GetHash("new@example.com") &&
            crypto.GetDecryptedText(x.WebsitePasswordFer) == "new-pass" &&
            x.WebsitePasswordSha == new Sha1Repository().GetHash("new-pass"));
    }
}
