using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using PasswordVault.Common.Database;
using PasswordVault.Common.Interfaces;
using PasswordVault.Common.Models;
using PasswordVault.Common.Services;

namespace PasswordVault.Users;

public class CreateUserRequest
{
    [Required]
    [EmailAddress]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}

public class UserProfileResponse
{
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Password { get; set; }
}

public class UserService
{
    private readonly AppDbContext _dbContext;
    private readonly IHash _hash;
    private readonly ICrypto _crypto;

    public UserService(AppDbContext dbContext, IHash? hash = null, ICrypto? crypto = null)
    {
        _dbContext = dbContext;
        _hash = hash ?? new HashArgon2();
        _crypto = crypto ?? new CryptoFernet("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=");
    }

    public async Task<UserModel> CreateUserAsync(CreateUserRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var username = request.Username.Trim();
        if (string.IsNullOrWhiteSpace(username) || !new EmailAddressAttribute().IsValid(username))
        {
            throw new ArgumentException("A valid email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        {
            throw new ArgumentException("First name and last name are required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        {
            throw new ArgumentException("Password must be at least 8 characters long.");
        }

        var usernameHash = new HashSha256().GetHash(username);
        var existingUser = await _dbContext.Users
            .AnyAsync(user => user.UsernameSha == usernameHash || user.UsernameFer == username);

        if (existingUser)
        {
            throw new ArgumentException("A user with that username already exists.");
        }

        var user = new UserModel
        {
            UsernameFer = _crypto.GetEncryptedText(username),
            UsernameSha = usernameHash,
            NameFer = _crypto.GetEncryptedText(request.FirstName.Trim()),
            LastNameFer = _crypto.GetEncryptedText(request.LastName.Trim()),
            Password = _hash.GetHash(request.Password),
            is_temporal = true,
            isActive = true
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        user.UsernameFer = username;
        user.NameFer = request.FirstName.Trim();
        user.LastNameFer = request.LastName.Trim();

        return user;
    }

    public async Task<UserProfileResponse?> GetCurrentUserAsync(int userId)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(record => record.Id == userId && record.isActive);

        if (user is null)
        {
            return null;
        }

        return new UserProfileResponse
        {
            Username = DecryptValue(user.UsernameFer),
            FirstName = DecryptValue(user.NameFer),
            LastName = DecryptValue(user.LastNameFer),
            Password = null
        };
    }

    private string DecryptValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        try
        {
            return _crypto.GetDecryptedText(value);
        }
        catch
        {
            return value;
        }
    }
}
