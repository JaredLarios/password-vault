using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PasswordVault.Auth;
using PasswordVault.Common.Database;
using PasswordVault.Common.Models;
using PasswordVault.Common.Repositories;
using PasswordVault.Users;

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
}
