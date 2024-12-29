using System.ComponentModel.DataAnnotations.Schema;

namespace BaseLibrary.Entities;

[Table("tbl_system_role")]
public class SystemRole
{
    public int Id { get; set; }
    public string? Name { get; set; }
}