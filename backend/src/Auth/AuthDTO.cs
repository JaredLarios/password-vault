using System.ComponentModel.DataAnnotations;

namespace PasswordVault.Auth;

public class AuthDTO{
    [Required]
    [EmailAddress]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;
    [Required]
    [StringLength(15)]
    public string Password { get; set; } = string.Empty;
};
