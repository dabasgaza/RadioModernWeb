// ============================================================
// AccountController — الحسابات
// ============================================================
// المسؤولية: تعريف الحسابات.
// ============================================================
using Domain.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Radio.Web.Controllers;

/// <summary>
/// صنف الحسابات.
/// </summary>
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    /// <summary>
    /// تسجيل دخول الحسابات.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    /// <summary>
    /// تسجيل دخول الحسابات.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        if (!ModelState.IsValid)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Username, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("تسجيل دخول ناجح للمستخدم {Username}", model.Username);
            return LocalRedirect(returnUrl);
        }

        if (result.RequiresTwoFactor)
            return RedirectToAction("LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });

        if (result.IsLockedOut)
        {
            _logger.LogWarning("تم تأمين حساب المستخدم {Username}", model.Username);
            ModelState.AddModelError(string.Empty, "تم تأمين حسابك مؤقتاً بسبب المحاولات الفاشلة. حاول لاحقاً.");
        }
        else if (result.IsNotAllowed)
            ModelState.AddModelError(string.Empty, "حسابك معطّل. يرجى التواصل مع مسؤول النظام.");
        else
            ModelState.AddModelError(string.Empty, "اسم المستخدم أو كلمة المرور غير صحيحة.");

        ViewData["ReturnUrl"] = returnUrl;
        return View(model);
    }

    /// <summary>
    /// تسجيل خروج الحسابات.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("تسجيل خروج ناجح");
        return RedirectToAction("Login");
    }

    /// <summary>
    /// Access Denied.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    /// <summary>
    /// Profile.
    /// </summary>
    [HttpGet]
    public IActionResult Profile()
    {
        // TODO: load user profile
        return View();
    }

    /// <summary>
    /// Settings.
    /// </summary>
    [HttpGet]
    public IActionResult Settings()
    {
        // TODO: load user settings
        return View();
    }
}

/// <summary>
/// صنف LoginViewModel.
/// </summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "اسم المستخدم مطلوب.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور مطلوبة.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "تذكّرني")]
    public bool RememberMe { get; set; }
}
