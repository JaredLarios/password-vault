using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PasswordVault.Common.Database;
using PasswordVault.Common.Services;

namespace PasswordVault.Auth;

public class AuthService
{
    private readonly IConfiguration _config;
    private readonly AppDbContext? _dbContext;

    public AuthService(IConfiguration config, AppDbContext? dbContext = null)
    {
        _config = config;
        _dbContext = dbContext;
    }

    public async Task<(string Token, CookieOptions CookieOptions)> GetAuthTokenAsync(AuthDTO credentials)
    {
        if (string.IsNullOrWhiteSpace(credentials.Username) || string.IsNullOrWhiteSpace(credentials.Password))
        {
            throw new ArgumentException("Wrong User or Password.");
        }

        if (_dbContext is not null)
        {
            var user = await _dbContext.Users
                .SingleOrDefaultAsync(u => u.UsernameFer == credentials.Username.Trim());

            if (user is null)
            {
                throw new ArgumentException("Wrong User or Password.");
            }

            var passwordHasher = new HashArgon2();
            if (!passwordHasher.CompareHash(credentials.Password, user.Password))
            {
                throw new ArgumentException("Wrong User or Password.");
            }
        }

        IConfigurationSection jwtSettings = _config.GetSection("Jwt");
        string jwtKey = _config["JWT_SECRET_KEY"] ??
            throw new KeyNotFoundException("JWT key not found");

        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        SigningCredentials signCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var userId = _dbContext is not null
            ? (await _dbContext.Users.SingleOrDefaultAsync(u => u.UsernameFer == credentials.Username.Trim()))?.Id
            : null;

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, credentials.Username),
            new Claim(ClaimTypes.NameIdentifier, userId?.ToString() ?? "0"),
            new Claim(ClaimTypes.Email, credentials.Username),
            new Claim(ClaimTypes.Role, "User")
        };

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: signCredentials
        );

        string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        CookieOptions cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddMinutes(15)
        };

        return (Token: tokenString, CookieOptions: cookieOptions);
    }
}