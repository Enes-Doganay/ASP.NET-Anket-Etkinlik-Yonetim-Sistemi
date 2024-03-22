using System.ComponentModel.DataAnnotations;

namespace Basics.Models;

public class LoginModel
{
    private string? _returnurl;
    [Required(ErrorMessage = "Email is required")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public string? Password { get; set; }

    public string ReturnUrl
    {
        get
        {
            if (_returnurl is null)
                return "/";
            else
                return _returnurl;
        }
        set
        {
            _returnurl = value;
        }
    }
}