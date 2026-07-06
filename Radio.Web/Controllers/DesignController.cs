using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Radio.Web.Controllers;

[Authorize]
public class DesignController : Controller
{
    public IActionResult Index() => View();
}
