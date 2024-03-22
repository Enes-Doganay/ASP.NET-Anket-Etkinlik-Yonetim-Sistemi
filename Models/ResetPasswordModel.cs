using System.ComponentModel.DataAnnotations;

public class ResetPasswordModel
{
    [Required]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Şifreler eşleşmedi")]
    public string? ConfirmPassword { get; set; }

    public string? Email { get; set; }
    public string? Token { get; set; }
}