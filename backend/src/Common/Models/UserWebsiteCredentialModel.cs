using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PasswordVault.Common.Models;

[Table("user_website_credential", Schema = "public")]
public class UserWebsiteCredentialModel : BaseModel
{
    [Key]
    [Column("user_website_credential_id", TypeName = "bigint")]
    public long Id { get; set; }

    [Column("user_website_credential_uuid", TypeName = "varchar")]
    public string Uuid { get; set; } = Guid.NewGuid().ToString();

    [Column("user_website_credential_username_fer", TypeName = "varchar")]
    public string UsernameFer { get; set; } = string.Empty;

    [Column("user_website_credential_username_sha", TypeName = "varchar(64)")]
    public string UsernameSha { get; set; } = string.Empty;

    [Column("user_website_credential_password_fer", TypeName = "varchar")]
    public string PasswordFer { get; set; } = string.Empty;

    [Column("user_website_credential_password_sha", TypeName = "varchar(64)")]
    public string PasswordSha { get; set; } = string.Empty;

    [Column("user_website_id", TypeName = "bigint")]
    public long WebsiteId { get; set; }

    [Column("is_secure")]
    public bool? IsSecure { get; set; }

    [ForeignKey(nameof(WebsiteId))]
    public WebsiteModel Website { get; set; } = null!;
}