using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.Intrinsics.Arm;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PasswordVault.Common.Database;
using PasswordVault.Common.Interfaces;
using PasswordVault.Common.Models;

namespace PasswordVault.Auth;

public class AuthService
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _context;
    private readonly ICrypto _crypto;
    private readonly IHash _argon2;
    private readonly IHash _sha256;

    public AuthService(
        IConfiguration config,
        AppDbContext context,
        [FromKeyedServices("services")] ICrypto crypto,
        [FromKeyedServices("argon2")] IHash argon2,
        [FromKeyedServices("sha256")] IHash sha256
    )
    {
        _config = config;
        _context = context;
        _crypto = crypto;
        _argon2 = argon2;
        _sha256 = sha256;
    }

    private async Task<UserModel?> GetUserByUsernameSha(string usernameSha)
    {
        return await _context
            .Users
            .Where(user => user.UsernameSha == usernameSha)
            .FirstOrDefaultAsync();
    }

    public async Task<(string Token, CookieOptions CookieOptions)> GetAuthTokenAsync(AuthDTO credentials)
    {
        string usernameSha = _sha256.GetHash(credentials.Username);

        UserModel? user = await GetUserByUsernameSha(usernameSha);
        if (user == null) throw new ArgumentException("Bad Username or Password");

        bool isRightPassword = _argon2.CompareHash(credentials.Password, user.Password);
        if (!isRightPassword) throw new ArgumentException("Bad Username or Password");

        return await SetUpToken(user.Uuid.ToString());
    }

    private async Task<(string Token, CookieOptions CookieOptions)> SetUpToken(string userId)
    {
        IConfigurationSection jwtSettings = _config.GetSection("Jwt");
        string jwtKey = _config["JWT_SECRET_KEY"] ??
            throw new KeyNotFoundException("JWT key not found");

        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        SigningCredentials signCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, userId),
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