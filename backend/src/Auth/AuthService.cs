using System.IdentityModel.Tokens.Jwt;
using System.Runtime.Intrinsics.Arm;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PasswordVault.Auth;

public class AuthService
{
    private readonly IConfiguration _config;

    public AuthService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<(string Token, CookieOptions CookieOptions)> GetAuthTokenAsync(AuthDTO credentials)
    {
        if (credentials.Username == string.Empty || credentials.Password == string.Empty)
        {
            throw new ArgumentException("Wrong User or Password.");
        }

        IConfigurationSection jwtSettings = _config.GetSection("Jwt");
        string jwtKey = _config["JWT_SECRET_KEY"] ??
            throw new KeyNotFoundException("JWT key not found");

        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        SigningCredentials signCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, credentials.Username),
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