using System.ComponentModel.DataAnnotations;

namespace PasswordVault.Users;

public class CreateUserDTO
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
    [StringLength(15, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}

public class UserProfileDTO
{
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class UserCreatedResponseDTO
{
    public string Message { get; set; } = string.Empty;
    public int UserId { get; set; }
}

public class UserMessageResponseDTO
{
    public string Message { get; set; } = string.Empty;
}
