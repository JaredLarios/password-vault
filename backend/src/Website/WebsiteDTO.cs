using System.ComponentModel.DataAnnotations;

namespace PasswordVault.Website;

public class NewWebsiteDTO
{
    [Required]
    [StringLength(50)]
    public string WebsiteName { get; set; } = string.Empty;

    [Required]
    [Url]
    [StringLength(2048)]
    public string WebsiteUrl { get; set; } = string.Empty;

    [Required]
    [StringLength(420)]
    public string WebsiteUsername { get; set; } = string.Empty;

    [Required]
    [StringLength(420)]
    public string WebsitePassword { get; set; } = string.Empty;
}

// Responses
public class NewWebsiteResponse
{
    public string Message { get; set; } = string.Empty;
}
