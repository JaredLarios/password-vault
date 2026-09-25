using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PasswordVault.Common.Models;

[Table("user_website_credential", Schema = "public")]
public class WebsiteLinkModel : BaseModel
{
    [Key]
    [Column("user_website_credential_id")]
    public int Id { get; set; }

    [Column("user_website_credential_uuid")]
    public Guid Uuid { get; set; } = Guid.NewGuid();

    [Column("user_website_credential_username_fer", TypeName = "varchar")]
    public string WebsiteUsernameFer { get; set; } = string.Empty;

    [Column("user_website_credential_username_sha", TypeName = "varchar")]
    public string WebsiteUsernameSha { get; set; } = string.Empty;

    [Column("user_website_credential_password_fer", TypeName = "varchar")]
    public string WebsitePasswordFer { get; set; } = string.Empty;

    [Column("user_website_credential_password_sha", TypeName = "varchar")]
    public string WebsitePasswordSha { get; set; } = string.Empty;

    [Column("user_website_id")]
    public int WebsiteId { get; set; }

    [ForeignKey(nameof(WebsiteId))]
    public WebsiteModel UserWebsite { get; set; } = null!;
}