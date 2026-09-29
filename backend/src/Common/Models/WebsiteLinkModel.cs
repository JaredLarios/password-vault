using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PasswordVault.Common.Models;

[Table("user_website_link", Schema = "public")]
public class WebsiteLinkModel : BaseModel
{
    [Key]
    [Column("user_website_link_id")]
    public int Id { get; set; }

    [Column("user_website_link_uuid")]
    public Guid Uuid { get; set; } = Guid.NewGuid();

    [Column("user_website_link_url", TypeName = "varchar(240)")]
    public string WebsiteUrl { get; set; } = string.Empty;

    [Column("user_website_id")]
    public int WebsiteId { get; set; }

    [ForeignKey(nameof(WebsiteId))]
    public WebsiteModel Website { get; set; } = null!;
}