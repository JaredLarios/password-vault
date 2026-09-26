using System.ComponentModel.DataAnnotations;

namespace PasswordVault.Websites;

public class WebsiteListCredentialResponse
{
    public Guid WebsiteUserId { get; set; }
    public string WebsiteUsername { get; set; } = string.Empty;
    public string WebistePassword { get; set; } = string.Empty;
}

public class WebsiteListResponse
{
    public string WebsiteName { get; set; } = string.Empty;
    public List<string> Urls { get; set; } = [];
    public List<WebsiteListCredentialResponse> Credentials { get; set; } = [];
}

public class UpdateWebsiteDto
{
    [StringLength(2048)]
    public string? WebsiteName { get; set; }

    [StringLength(2048)]
    public string? WebsiteUrl { get; set; }

    [StringLength(420)]
    public string? WebsiteUsername { get; set; }

    [StringLength(420)]
    public string? WebistePassword { get; set; }
}

public class WebsiteUpdateResponse
{
    public string Message { get; set; } = "Website credentials updated successfully.";
}
