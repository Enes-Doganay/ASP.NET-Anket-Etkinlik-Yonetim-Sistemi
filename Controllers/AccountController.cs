using Basics.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signManager;
    private readonly ILogger<AccountController> _logger;
    private readonly IEmailSender _emailSender;
    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signManager, ILogger<AccountController> logger, IEmailSender emailSender)
    {
        _userManager = userManager;
        _signManager = signManager;
        _logger = logger;
        _emailSender = emailSender;
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login([FromForm] LoginModel model)
    {
        if (ModelState.IsValid)
        {
            ApplicationUser user = await _userManager.FindByEmailAsync(model.Email);
            if (user is not null)
            {
                await _signManager.SignOutAsync();
                if ((await _signManager.PasswordSignInAsync(user, model.Password, false, false)).Succeeded)
                {
                    return Redirect(model?.ReturnUrl ?? "/");
                }
            }
            ModelState.AddModelError("Error", "Kullanıcı adı veya şifre geçersiz");
        }
        return View();
    }

    public async Task<IActionResult> Logout([FromQuery(Name = "ReturnUrl")] string ReturnUrl = "/")
    {
        await _signManager.SignOutAsync();
        return Redirect(ReturnUrl);
    }

    public IActionResult Signup()
    {
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Signup([FromForm] SignupDto model)
    {
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            var roleResult = await _userManager.AddToRoleAsync(user, "User");

            if (roleResult.Succeeded)
                return RedirectToAction("Login", new { ReturnUrl = "/" });
        }
        else
        {
            foreach (var err in result.Errors)
            {
                ModelState.AddModelError("", err.Description);
                _logger.LogError(err.Description);
            }
        }

        return View();
    }
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordModel forgotPasswordModel)
    {
        if (!ModelState.IsValid)
            return View(forgotPasswordModel);

        var user = await _userManager.FindByEmailAsync(forgotPasswordModel.Email);
        if (user == null)
            return RedirectToAction(nameof(ForgotPasswordConfirmation));

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var callback = Url.Action(nameof(ResetPassword), "Account", new { token, email = user.Email }, Request.Scheme);

        var message = new Message(new string[] { user.Email }, "Reset password token", callback); //Message 3 parametreydi 1 tane daha ekledim daha olabilir
        await _emailSender.SendEmailAsync(message);

        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }
    [HttpGet]
    public IActionResult ResetPassword(string token, string email)
    {
        var model = new ResetPasswordModel { Token = token, Email = email };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return View("ResetPasswordConfirmation");

        var resetPassResult = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
        if (!resetPassResult.Succeeded)
        {
            foreach (var error in resetPassResult.Errors)
            {
                ModelState.TryAddModelError(error.Code, error.Description);
            }
            return View();
        }
        return View("ResetPasswordConfirmation");
    }


    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

}