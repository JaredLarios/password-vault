using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PasswordVault.Common.Models;

[Table("user_website_link", Schema = "public")]
public class UserWebsiteLinkModel : BaseModel
{
    [Key]
    [Column("user_website_link_id", TypeName = "bigint")]
    public long Id { get; set; }

    [Column("user_website_link_uuid", TypeName = "varchar")]
    public string Uuid { get; set; } = Guid.NewGuid().ToString();

    [Column("user_website_link_url", TypeName = "varchar")]
    public string Url { get; set; } = string.Empty;

    [Column("user_website_id", TypeName = "bigint")]
    public long WebsiteId { get; set; }

    [ForeignKey(nameof(WebsiteId))]
    public WebsiteModel Website { get; set; } = null!;
}