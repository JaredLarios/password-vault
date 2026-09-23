using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PasswordVault.Auth;
using PasswordVault.Common.Database;
using PasswordVault.Common.Models;
using PasswordVault.Common.Repositories;
using PasswordVault.Users;
using PasswordVault.Websites;

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
        var service = new UserService(context, new FernetRepository("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA="), new Argon2Repository(), new Sha256Repository());

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
        var user = new UserModel
        {
            UsernameFer = "fabian@example.com",
            UsernameSha = "sha256",
            NameFer = "Fabian",
            LastNameFer = "Betancourt",
            Password = "hashed-password",
            isActive = true
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var crypto = new FernetRepository("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=");
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
        var service = new UserService(context, new FernetRepository("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA="), new Argon2Repository(), new Sha256Repository());
        var controller = new UserController(service);

        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var action = await controller.GetProfile();

        Assert.IsType<UnauthorizedResult>(action);
    }

    [Fact]
    public async Task AuthRequiredMiddleware_AllowsAuthenticatedRequest()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);
        var user = new UserModel
        {
            UsernameFer = "fabian@example.com",
            UsernameSha = "sha256",
            NameFer = "Fabian",
            LastNameFer = "Betancourt",
            Password = "hashed-password",
            isActive = true
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new UserService(context, new FernetRepository("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA="), new Argon2Repository(), new Sha256Repository());
        var controller = new UserController(service);

        var httpContext = new DefaultHttpContext();
        httpContext.Items["UserId"] = user.Id;
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var action = await controller.GetProfile();

        var ok = Assert.IsType<OkObjectResult>(action);
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
        context.WebsiteUrls.Add(new WebsiteUrlModel
        {
            WebsiteId = website.Id,
            Url = "https://www.facebook.com"
        });
        context.WebsiteCredentials.Add(new WebsiteCredentialModel
        {
            WebsiteId = website.Id,
            WebsiteUsername = crypto.GetEncryptedText("user@example.com"),
            WebsitePassword = crypto.GetEncryptedText("secret-pass"),
            WebsiteUserId = Guid.NewGuid()
        });
        await context.SaveChangesAsync();

        var service = new WebsiteService(context, crypto);

        var result = await service.GetWebsitesAsync(user.Uuid, website.Uuid);

        var item = Assert.Single(result);
        Assert.Equal("Facebook", item.WebsiteName);
        Assert.Contains("https://www.facebook.com", item.Urls);
        var credential = Assert.Single(item.Credentials);
        Assert.Equal("user@example.com", credential.WebsiteUsername);
        Assert.Equal("secret-pass", credential.WebistePassword);
    }

    [Fact]
    public async Task WebsiteService_UpdateWebsite_UpdatesWebsiteNameAndCredentialValues()
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
            WebsiteName = "Old Name"
        };
        context.Websites.Add(website);
        context.WebsiteUrls.Add(new WebsiteUrlModel
        {
            WebsiteId = website.Id,
            Url = "https://old.example.com"
        });
        var credential = new WebsiteCredentialModel
        {
            WebsiteId = website.Id,
            WebsiteUsername = crypto.GetEncryptedText("old@example.com"),
            WebsitePassword = crypto.GetEncryptedText("old-pass"),
            WebsiteUserId = Guid.NewGuid()
        };
        context.WebsiteCredentials.Add(credential);
        await context.SaveChangesAsync();

        var service = new WebsiteService(context, crypto);

        var response = await service.UpdateWebsiteAsync(
            user.Uuid,
            website.Uuid,
            new UpdateWebsiteRequest
            {
                WebsiteName = "New Name",
                WebsiteUrl = "https://new.example.com",
                WebsiteUsername = "new@example.com",
                WebistePassword = "new-pass"
            });

        Assert.Equal("Website credentials updated successfully.", response.Message);

        var updated = await context.Websites.Include(x => x.Urls).Include(x => x.Credentials)
            .SingleAsync(x => x.Uuid == website.Uuid);
        Assert.Equal("New Name", updated.WebsiteName);
        Assert.Contains(updated.Urls, x => x.Url == "https://new.example.com");
        Assert.Contains(updated.Credentials, x =>
            crypto.GetDecryptedText(x.WebsiteUsername) == "new@example.com" &&
            crypto.GetDecryptedText(x.WebsitePassword) == "new-pass");
    }
}
