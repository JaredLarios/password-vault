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

public class UpdateWebsiteDTO
{
    [StringLength(2048)]
    public string? WebsiteName { get; set; }

    [StringLength(2048)]
    public string? WebsiteUrl { get; set; }

    [StringLength(420)]
    public string? WebsiteUsername { get; set; }

    [StringLength(420)]
    public string? WebsitePassword { get; set; }
}

// Responses
public class NewWebsiteResponse
{
    public string Message { get; set; } = string.Empty;
}

public class WebsiteListCredentialResponse
{
    public Guid WebsiteUserId { get; set; }
    public string WebsiteUsername { get; set; } = string.Empty;
    public string WebsitePassword { get; set; } = string.Empty;
}

public class WebsiteListResponse
{
    public Guid WebsiteId { get; set; }
    public string WebsiteName { get; set; } = string.Empty;
    public List<string> Urls { get; set; } = [];
    public List<WebsiteListCredentialResponse> Credentials { get; set; } = [];
}
