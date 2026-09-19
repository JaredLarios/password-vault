using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace PasswordVault.Users;

public class NewUserDTO
{
    [Required]
    [EmailAddress]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [StringLength(30)]
    public string? Name { get; set; }

    [StringLength(30)]
    public string? LastName { get; set; }

    [Required]
    [StringLength(15)]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}

// Responses
public class NewUserResponse
{
    public string Message { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}

public class UserProfileResponse
{
    public string Username { get; set; } = string.Empty;
    public string? Name { get; set; } = string.Empty;
    public string? LastName { get; set; } = string.Empty;
}
