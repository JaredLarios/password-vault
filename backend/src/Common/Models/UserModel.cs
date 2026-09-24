using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PasswordVault.Common.Models;

[Table("sys_user", Schema = "public")]
public class UserModel : BaseModel
{
    [Key]
    [Column("sys_user_id")]
    public int Id { get; set; }

    [Column("sys_user_uuid")]
    public Guid Uuid { get; set; } = Guid.NewGuid();

    [Column("sys_user_username_fer", TypeName = "varchar(420)")]
    public string UsernameFer { get; set; } = string.Empty;

    [Column("sys_user_username_sha", TypeName = "varchar(64)")]
    public string UsernameSha { get; set; } = string.Empty;

    [Column("sys_user_name_fer", TypeName = "varchar(420)")]
    public string? NameFer { get; set; } = string.Empty;

    [Column("sys_user_last_name_fer", TypeName = "varchar(420)")]
    public string? LastNameFer { get; set; } = string.Empty;

    [Column("sys_user_password", TypeName = "varchar(128)")]
    public string Password { get; set; } = string.Empty;

    [Column("is_temporal")]
    public bool isTemporal { get; set; } = true;

    public List<WebsiteModel> Websites { get; set; } = [];
}
