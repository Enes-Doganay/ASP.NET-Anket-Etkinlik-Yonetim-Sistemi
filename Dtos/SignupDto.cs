using System.ComponentModel.DataAnnotations;

public record SignupDto
{
    [Required(ErrorMessage ="İsim gerekli")]
    public string? FirstName { get; init; }
    [Required(ErrorMessage ="Soyisim gerekli")]
    public string? LastName { get; init; }
    [Required(ErrorMessage ="Email gerekli")]
    public string? Email { get; init; }
    [Required(ErrorMessage ="Şifre gerekli")]
    public string? Password { get; init; }
}