using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PasswordVault.Auth;
using PasswordVault.Common.Database;
using PasswordVault.Common.Models;
using PasswordVault.Common.Repositories;

namespace PasswordVaultAPI.Tests;

public class AuthTests
{
    private const string SecretKey = "test-secret-key-that-is-long-enough-for-hmac-sha256";

    [Fact]
    public async Task AuthService_ReturnsSignedTokenAndSecureCookieOptions()
    {
        var service = CreateService(includeSecret: true);
        var credentials = new AuthDTO { Username = "user@example.com", Password = "password" };

        (string token, CookieOptions cookieOptions) = await service.GetAuthTokenAsync(credentials);
        var parsedToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.NotEmpty(parsedToken.Claims.Single(claim => claim.Type == ClaimTypes.Name).Value);
        Assert.Equal("User", parsedToken.Claims.Single(claim => claim.Type == ClaimTypes.Role).Value);
        Assert.Equal("issuer", parsedToken.Issuer);
        Assert.Equal("audience", parsedToken.Audiences.Single());
        Assert.True(parsedToken.ValidTo > DateTime.UtcNow.AddMinutes(14));
        Assert.True(cookieOptions.HttpOnly);
        Assert.True(cookieOptions.Secure);
        Assert.Equal(SameSiteMode.None, cookieOptions.SameSite);
        Assert.True(cookieOptions.Expires > DateTimeOffset.UtcNow.AddMinutes(14));
    }

    [Theory]
    [InlineData("", "password")]
    [InlineData("user@example.com", "")]
    public async Task AuthService_RejectsMissingCredentials(string username, string password)
    {
        var service = CreateService(includeSecret: true, seedUser: false);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetAuthTokenAsync(new AuthDTO { Username = username, Password = password }));

        Assert.Equal("Wrong User or Password.", exception.Message);
    }

    [Fact]
    public async Task AuthService_ThrowsWhenJwtSecretIsMissing()
    {
        var service = CreateService(includeSecret: false);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.GetAuthTokenAsync(new AuthDTO { Username = "user@example.com", Password = "password" }));
    }

    [Fact]
    public async Task AuthController_LoginReturnsOkAndSetsCookie()
    {
        var controller = CreateController(includeSecret: true);

        IActionResult action = await controller.Login(new AuthDTO
        {
            Username = "user@example.com",
            Password = "password"
        });

        var result = Assert.IsType<OkObjectResult>(action);
        Assert.Equal("Logged in successfully", GetMessage(result.Value));
        Assert.Contains("auth_token=", controller.HttpContext.Response.Headers.SetCookie.ToString());
    }

    [Fact]
    public async Task AuthController_LoginReturnsBadRequestForInvalidCredentials()
    {
        var controller = CreateController(includeSecret: true);

        IActionResult action = await controller.Login(new AuthDTO());

        var result = Assert.IsType<BadRequestObjectResult>(action);
        Assert.Equal("Wrong User or Password.", GetMessage(result.Value));
    }

    [Fact]
    public async Task AuthController_LoginReturnsServerErrorForUnexpectedFailures()
    {
        var controller = CreateController(includeSecret: false);

        IActionResult action = await controller.Login(new AuthDTO
        {
            Username = "user@example.com",
            Password = "password"
        });

        var result = Assert.IsType<ObjectResult>(action);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        Assert.Equal("An unexpected error occurred.", GetMessage(result.Value));
    }

    [Fact]
    public async Task AuthController_ChangePasswordUpdatesAuthenticatedUserPassword()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new AppDbContext(options);
        var argon2 = new Argon2Repository();
        var user = new UserModel
        {
            UsernameSha = new Sha256Repository().GetHash("user@example.com"),
            Password = argon2.GetHash("oldpassword"),
            isActive = true
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new AuthService(
            CreateConfiguration(includeSecret: true),
            context,
            argon2,
            new Sha256Repository());
        var controller = new AuthController(service);
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, user.Uuid.ToString())],
            "Test"));
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        IActionResult action = await controller.ChangePassword(new NewPasswordDTO
        {
            NewPassword = "newpassword1"
        });

        var result = Assert.IsType<OkObjectResult>(action);
        Assert.Equal("Password updated Successfully", GetMessage(result.Value));
        Assert.True(argon2.CompareHash("newpassword1", context.Users.Single().Password));
        Assert.False(argon2.CompareHash("oldpassword", context.Users.Single().Password));
    }

    [Fact]
    public async Task AuthController_ChangePasswordReturnsBadRequestForInvalidPasswordLength()
    {
        var controller = CreateController(includeSecret: true);
        controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, Guid.NewGuid().ToString())],
            "Test"));

        IActionResult action = await controller.ChangePassword(new NewPasswordDTO
        {
            NewPassword = "short"
        });

        var result = Assert.IsType<BadRequestObjectResult>(action);
        Assert.Equal("Password must be between 10 and 15 characters.", GetMessage(result.Value));
    }

    [Fact]
    public void AuthController_LogoutDeletesCookie()
    {
        var controller = CreateController(includeSecret: true);

        IActionResult action = controller.logout();

        var result = Assert.IsType<OkObjectResult>(action);
        Assert.Equal("Logged out successfully", GetMessage(result.Value));
        Assert.Contains("auth_token=", controller.HttpContext.Response.Headers.SetCookie.ToString());
    }

    private static AuthController CreateController(bool includeSecret)
    {
        var controller = new AuthController(CreateService(includeSecret));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    private static AuthService CreateService(bool includeSecret, bool seedUser = true)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(options);

        if (seedUser)
        {
            context.Users.Add(new UserModel
            {
                UsernameSha = new Sha256Repository().GetHash("user@example.com"),
                Password = new Argon2Repository().GetHash("password"),
                isActive = true
            });
            context.SaveChanges();
        }

        return new AuthService(
            CreateConfiguration(includeSecret),
            context,
            new Argon2Repository(),
            new Sha256Repository());
    }

    private static IConfiguration CreateConfiguration(bool includeSecret)
    {
        var values = new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = "issuer",
            ["Jwt:Audience"] = "audience"
        };
        if (includeSecret)
        {
            values["JWT_SECRET_KEY"] = SecretKey;
        }
        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    private static string? GetMessage(object? value) =>
        value?.GetType().GetProperty("Message")?.GetValue(value)?.ToString();
}
