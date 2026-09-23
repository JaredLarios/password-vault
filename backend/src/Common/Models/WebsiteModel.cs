using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PasswordVault.Common.Models;

[Table("user_website", Schema = "public")]
public class WebsiteModel : BaseModel
{
    [Key]
    [Column("user_website_id")]
    public int Id { get; set; }

    [Column("user_website_uuid")]
    public Guid Uuid { get; set; } = Guid.NewGuid();

    [Column("user_website_name", TypeName = "varchar")]
    public string WebsiteName { get; set; } = string.Empty;

    [Column("sys_user_id")]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public UserModel User { get; set; } = null!;

    public ICollection<WebsiteUrlModel> Urls { get; set; } = new List<WebsiteUrlModel>();

    public ICollection<WebsiteCredentialModel> Credentials { get; set; } = new List<WebsiteCredentialModel>();
}

[Table("website_url", Schema = "public")]
public class WebsiteUrlModel : BaseModel
{
    [Key]
    [Column("website_url_id")]
    public int Id { get; set; }

    [Column("website_url", TypeName = "varchar(2048)")]
    public string Url { get; set; } = string.Empty;

    [Column("user_website_id")]
    public int WebsiteId { get; set; }

    [ForeignKey(nameof(WebsiteId))]
    public WebsiteModel Website { get; set; } = null!;
}

[Table("website_credential", Schema = "public")]
public class WebsiteCredentialModel : BaseModel
{
    [Key]
    [Column("website_credential_id")]
    public int Id { get; set; }

    [Column("website_user_uuid")]
    public Guid WebsiteUserId { get; set; } = Guid.NewGuid();

    [Column("website_username_fer", TypeName = "varchar(420)")]
    public string WebsiteUsername { get; set; } = string.Empty;

    [Column("website_password_fer", TypeName = "varchar(420)")]
    public string WebsitePassword { get; set; } = string.Empty;

    [Column("user_website_id")]
    public int WebsiteId { get; set; }

    [ForeignKey(nameof(WebsiteId))]
    public WebsiteModel Website { get; set; } = null!;
}
