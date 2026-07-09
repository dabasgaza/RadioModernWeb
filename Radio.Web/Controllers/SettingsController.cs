using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Radio.Web.Services;

namespace Radio.Web.Controllers;

[Authorize]
public class SettingsController(IEmailService email, IConfiguration config) : Controller
{
    public IActionResult Index()
    {
        ViewBag.EmailConfigured = email.IsConfigured;
        ViewBag.SmtpHost = config["Email:Host"] ?? "(غير مضبوط)";
        ViewBag.SmtpPort = config["Email:Port"] ?? "(غير مضبوط)";
        return View();
    }
}
