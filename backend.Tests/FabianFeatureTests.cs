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
        var service = new UserService(context, new Argon2Repository(), new FernetRepository("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA="));

        var result = await service.CreateUserAsync(new CreateUserDTO
        {
            FirstName = "Fabian",
            LastName = "Betancourt",
            Username = "fabian@example.com",
            Password = "Password123!"
        });

        Assert.NotNull(result);
        Assert.NotEqual("fabian@example.com", result.UsernameFer);
        Assert.NotEqual("Password123!", result.Password);
        Assert.True(result.is_temporal);
        Assert.NotEqual(Guid.Empty, result.Uuid);
        Assert.Equal(1, await context.Users.CountAsync());
    }

    [Fact]
    public async Task RegisterUser_RejectsDuplicateUsername()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);
        var service = new UserService(context, new Argon2Repository(), new FernetRepository("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA="));

        await service.CreateUserAsync(new CreateUserDTO
        {
            FirstName = "Fabian",
            LastName = "Betancourt",
            Username = "fabian@example.com",
            Password = "Password123!"
        });

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateUserAsync(new CreateUserDTO
            {
                FirstName = "Fabian",
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

        var service = new UserService(context, new Argon2Repository(), new FernetRepository("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA="));
        var result = await service.GetCurrentUserAsync(user.Uuid);

        Assert.NotNull(result);
        Assert.Equal("fabian@example.com", result!.Username);
        Assert.Equal("Fabian", result.FirstName);
        Assert.Equal("Betancourt", result.LastName);
    }

    [Fact]
    public async Task AuthRequiredMiddleware_RejectsMissingToken()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);
        var service = new UserService(context, new Argon2Repository(), new FernetRepository("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA="));
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

        var service = new UserService(context, new Argon2Repository(), new FernetRepository("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA="));
        var controller = new UserController(service);

        var httpContext = new DefaultHttpContext();
        httpContext.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(
                [new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.Uuid.ToString())],
                "test"));
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var action = await controller.GetProfile();

        var ok = Assert.IsType<OkObjectResult>(action);
        var payload = Assert.IsType<UserProfileDTO>(ok.Value);
        Assert.Equal("fabian@example.com", payload.Username);
    }
}
