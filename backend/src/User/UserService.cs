using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PasswordVault.Common.Database;
using PasswordVault.Common.Interfaces;
using PasswordVault.Common.Models;
using PasswordVault.Common.Repositories;

namespace PasswordVault.Users;

public class UserService
{
    private readonly AppDbContext _dbContext;
    private readonly IHash _hash;
    private readonly ICrypto _crypto;

    public UserService(
        AppDbContext dbContext,
        [FromKeyedServices("services")] ICrypto crypto,
        [FromKeyedServices("argon2")] IHash hash,
        [FromKeyedServices("sha256")] IHash usernameHash)
    {
        _dbContext = dbContext;
        _hash = hash;
        _crypto = crypto;
        _usernameHash = usernameHash;
    }

    private readonly IHash _usernameHash;

    public UserService(AppDbContext dbContext, IHash hash, ICrypto crypto)
        : this(dbContext, crypto, hash, new Sha256Repository())
    {
    }

    public async Task<UserModel> CreateUserAsync(CreateUserDTO request)
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

        var usernameHash = _usernameHash.GetHash(username);
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

        return user;
    }

    public async Task<UserProfileDTO?> GetCurrentUserAsync(Guid userUuid)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(record => record.Uuid == userUuid && record.isActive);

        if (user is null)
        {
            return null;
        }

        return new UserProfileDTO
        {
            Username = DecryptValue(user.UsernameFer),
            FirstName = DecryptValue(user.NameFer),
            LastName = DecryptValue(user.LastNameFer)
        };
    }

    public async Task<UserProfileDTO?> GetCurrentUserAsync(ClaimsPrincipal principal)
    {
        var userUuidClaim = principal.FindFirstValue(ClaimTypes.Name);
        return Guid.TryParse(userUuidClaim, out var userUuid)
            ? await GetCurrentUserAsync(userUuid)
            : null;
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
