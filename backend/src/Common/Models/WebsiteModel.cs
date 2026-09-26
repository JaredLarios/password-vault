using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PasswordVault.Common.Models;

[Table("user_website", Schema = "public")]
public class WebsiteModel : BaseModel
{
    [Key]
    [Column("user_website_id", TypeName = "bigint")]
    public long Id { get; set; }

    [Column("user_website_uuid", TypeName = "varchar")]
    public Guid Uuid { get; set; } = Guid.NewGuid();

    [Column("user_website_name", TypeName = "varchar")]
    public string WebsiteName { get; set; } = string.Empty;

    [Column("sys_user_id", TypeName = "bigint")]
    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public UserModel User { get; set; } = null!;

    public ICollection<UserWebsiteLinkModel> Links { get; set; } = new List<UserWebsiteLinkModel>();

    public ICollection<UserWebsiteCredentialModel> Credentials { get; set; } = new List<UserWebsiteCredentialModel>();
}
