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

    public List<WebsiteLinkModel> WebsiteLinks { get; set; } = [];
    public List<WebsiteCredentialModel> WebsiteCredentials { get; set; } = [];
}
