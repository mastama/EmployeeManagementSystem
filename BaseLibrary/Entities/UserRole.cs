using System.ComponentModel.DataAnnotations.Schema;

namespace BaseLibrary.Entities;

[Table("tbl_user_role")]
public class UserRole
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public int UserId { get; set; }
}