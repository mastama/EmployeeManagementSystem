using System.ComponentModel.DataAnnotations.Schema;

namespace BaseLibrary.Entities;

[Table("tbl_refresh_token_info")]
public class RefreshTokenInfo
{
    public int Id { get; set; }
    public string? Token { get; set; }
    public int UserId { get; set; }
}