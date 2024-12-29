using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.DTOs;

public class Register : AccountBase
{
    [Required]
    [MinLength(3)]
    [MaxLength(255)]
    public string? Fullname { get; set; }
    
    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    public string? ConfirmPassword { get; set; }
}