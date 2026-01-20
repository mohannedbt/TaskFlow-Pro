using System.ComponentModel.DataAnnotations;
using TaskFlow_Pro.Models;

public class RegisterViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = null!;
    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = null!;

    [Required]
    public SignupRole Role { get; set; }

    // present if invited (link or manual paste)
    public string? InviteCode { get; set; }
    [Required]
    //Username
    public string Username { get; set; }
}