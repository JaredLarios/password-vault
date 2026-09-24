using System.ComponentModel.DataAnnotations;
using System.Runtime.Intrinsics.Arm;
using Microsoft.EntityFrameworkCore;
using PasswordVault.Common.Database;
using PasswordVault.Common.Interfaces;
using PasswordVault.Common.Models;

namespace PasswordVault.Users;

public class UserService
{
    private readonly AppDbContext _context;
    private readonly ICrypto _crypto;
    private readonly IHash _argon2;
    private readonly IHash _sha256;

    public UserService(
        AppDbContext context,
        [FromKeyedServices("services")] ICrypto crypto,
        [FromKeyedServices("argon2")] IHash argon2,
        [FromKeyedServices("sha256")] IHash sha256
    )
    {
        _context = context;
        _crypto = crypto;
        _argon2 = argon2;
        _sha256 = sha256;
    }

    private async Task<bool> ExistsUserWithUsernameShaAsync(string usernameSha)
    {
        return await _context
            .Users
            .AnyAsync(user => user.UsernameSha == usernameSha);
    }

    private async Task<UserModel?> GetUserByUuidAsync(Guid userUuid)
    {
        return await _context
            .Users
            .Where(user => user.Uuid == userUuid && user.isActive)
            .FirstOrDefaultAsync();
    }

    public async Task<NewUserResponse> CreateUserAsync(NewUserDTO newUser)
    {
        string usernameHash = _sha256.GetHash(newUser.Username);
        bool existingUser = await ExistsUserWithUsernameShaAsync(usernameHash);

        if (existingUser) throw new ArgumentException("A user with that username already exists.");

        UserModel user = new UserModel
        {
            Uuid = Guid.NewGuid(),
            UsernameFer = _crypto.GetEncryptedText(newUser.Username),
            UsernameSha = usernameHash,
            NameFer = string.IsNullOrEmpty(newUser.Name?.Trim()) ? 
                null : 
                _crypto.GetEncryptedText(newUser.Name.Trim()),
            LastNameFer = string.IsNullOrEmpty(newUser.LastName?.Trim()) ?
                null :
                _crypto.GetEncryptedText(newUser.LastName.Trim()),
            Password = _argon2.GetHash(newUser.Password),
            isTemporal = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new NewUserResponse
        {
            Message = "User created successfully",
            UserId = user.Uuid
        };
    }

    public async Task<UserProfileResponse> GetCurrentUserAsync(Guid userId)
    {
        UserModel? user = await GetUserByUuidAsync(userId);

        if (user == null) throw new KeyNotFoundException("User not found");

        return new UserProfileResponse
        {
            Username = _crypto.GetDecryptedText(user.UsernameFer),
            Name = string.IsNullOrEmpty(user.NameFer) ?
                null :
                _crypto.GetDecryptedText(user.NameFer),
            LastName = string.IsNullOrEmpty(user.LastNameFer) ?
                null :
                _crypto.GetDecryptedText(user.LastNameFer)
        };
    }
}
