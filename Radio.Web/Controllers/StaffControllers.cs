using DataAccess.Common;
using DataAccess.DTOs;
using DataAccess.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Radio.Web.Services;
using System.Threading;
using Radio.Web.ViewModels;

namespace Radio.Web.Controllers;

[Authorize]
public class EmployeesController : Controller
{
    private readonly IEmployeeService _employees;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(IEmployeeService employees, ICurrentUserService currentUser, ILogger<EmployeesController> logger)
    {
        _employees = employees; _currentUser = currentUser; _logger = logger;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var list = await _employees.GetAllActiveAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            list = list.Where(e => e.FullName?.Contains(s, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
        }
        ViewBag.Search = search ?? "";
        return View(list.OrderBy(e => e.FullName).ToList());
    }

    [Authorize(Policy = AppPermissions.StaffManage)]
    public async Task<IActionResult> Create()
    {
        ViewBag.StaffRoles = await _employees.GetAllRolesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        return View("Edit", new EmployeeDto(0, string.Empty, null, null, null));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.StaffManage)]
    public async Task<IActionResult> Create(EmployeeDto model)
    {
        if (!ModelState.IsValid) { ViewBag.StaffRoles = await _employees.GetAllRolesAsync(cancellationToken: HttpContext?.RequestAborted ?? default); return View("Edit", model); }
        var session = _currentUser.ToUserSession()!;
        var r = await _employees.CreateAsync(model, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) { TempData["Success"] = "تم إضافة الموظف"; return RedirectToAction(nameof(Index)); }
        ModelState.AddModelError("", r.ErrorMessage!);
        ViewBag.StaffRoles = await _employees.GetAllRolesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        return View("Edit", model);
    }

    [Authorize(Policy = AppPermissions.StaffManage)]
    public async Task<IActionResult> Edit(int id)
    {
        var list = await _employees.GetAllActiveAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var e = list.FirstOrDefault(x => x.EmployeeId == id);
        if (e == null) return NotFound();
        ViewBag.StaffRoles = await _employees.GetAllRolesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        return View(e);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.StaffManage)]
    public async Task<IActionResult> Edit(int id, EmployeeDto model)
    {
        if (!ModelState.IsValid) { ViewBag.StaffRoles = await _employees.GetAllRolesAsync(cancellationToken: HttpContext?.RequestAborted ?? default); return View(model); }
        model = model with { EmployeeId = id };
        var session = _currentUser.ToUserSession()!;
        var r = await _employees.UpdateAsync(model, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) { TempData["Success"] = "تم تحديث الموظف"; return RedirectToAction(nameof(Index)); }
        ModelState.AddModelError("", r.ErrorMessage!);
        ViewBag.StaffRoles = await _employees.GetAllRolesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.StaffManage)]
    public async Task<IActionResult> Delete(int id)
    {
        var session = _currentUser.ToUserSession()!;
        var r = await _employees.SoftDeleteAsync(id, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) TempData["Success"] = "تم حذف الموظف";
        else TempData["Error"] = r.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }
}

[Authorize]
public class StaffRolesController : Controller
{
    private readonly IEmployeeService _employees;
    private readonly ICurrentUserService _currentUser;

    public StaffRolesController(IEmployeeService employees, ICurrentUserService currentUser)
    {
        _employees = employees; _currentUser = currentUser;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _employees.GetAllRolesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        return View(list);
    }

    [Authorize(Policy = AppPermissions.StaffManage)]
    public IActionResult Create() => View("Edit", new StaffRoleDto(0, string.Empty));

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.StaffManage)]
    public async Task<IActionResult> Create(StaffRoleDto model)
    {
        if (!ModelState.IsValid) return View("Edit", model);
        var session = _currentUser.ToUserSession()!;
        var r = await _employees.CreateRoleAsync(model, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) { TempData["Success"] = "تم إضافة الدور"; return RedirectToAction(nameof(Index)); }
        ModelState.AddModelError("", r.ErrorMessage!);
        return View("Edit", model);
    }

    [Authorize(Policy = AppPermissions.StaffManage)]
    public async Task<IActionResult> Edit(int id)
    {
        var list = await _employees.GetAllRolesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var r = list.FirstOrDefault(x => x.StaffRoleId == id);
        if (r == null) return NotFound();
        return View(r);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.StaffManage)]
    public async Task<IActionResult> Edit(int id, StaffRoleDto model)
    {
        if (!ModelState.IsValid) return View(model);
        model = model with { StaffRoleId = id };
        var session = _currentUser.ToUserSession()!;
        var r = await _employees.UpdateRoleAsync(model, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) { TempData["Success"] = "تم تحديث الدور"; return RedirectToAction(nameof(Index)); }
        ModelState.AddModelError("", r.ErrorMessage!);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.StaffManage)]
    public async Task<IActionResult> Delete(int id)
    {
        var session = _currentUser.ToUserSession()!;
        var r = await _employees.SoftDeleteRoleAsync(id, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) TempData["Success"] = "تم حذف الدور";
        else TempData["Error"] = r.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }
}

[Authorize]
public class SocialPlatformsController : Controller
{
    private readonly IPlatformService _platforms;
    private readonly ICurrentUserService _currentUser;

    public SocialPlatformsController(IPlatformService platforms, ICurrentUserService currentUser)
    {
        _platforms = platforms; _currentUser = currentUser;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _platforms.GetAllActiveAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        return View(list);
    }

    [Authorize(Policy = AppPermissions.StaffManage)]
    public IActionResult Create() => View("Edit", new SocialMediaPlatformDto(0, string.Empty, null));

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.StaffManage)]
    public async Task<IActionResult> Create(SocialMediaPlatformDto model)
    {
        if (!ModelState.IsValid) return View("Edit", model);
        var session = _currentUser.ToUserSession()!;
        var r = await _platforms.CreateAsync(model, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) { TempData["Success"] = "تم إضافة المنصة"; return RedirectToAction(nameof(Index)); }
        ModelState.AddModelError("", r.ErrorMessage!);
        return View("Edit", model);
    }

    [Authorize(Policy = AppPermissions.StaffManage)]
    public async Task<IActionResult> Edit(int id)
    {
        var list = await _platforms.GetAllActiveAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var p = list.FirstOrDefault(x => x.SocialMediaPlatformId == id);
        if (p == null) return NotFound();
        return View(p);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.StaffManage)]
    public async Task<IActionResult> Edit(int id, SocialMediaPlatformDto model)
    {
        if (!ModelState.IsValid) return View(model);
        model = model with { SocialMediaPlatformId = id };
        var session = _currentUser.ToUserSession()!;
        var r = await _platforms.UpdateAsync(model, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) { TempData["Success"] = "تم تحديث المنصة"; return RedirectToAction(nameof(Index)); }
        ModelState.AddModelError("", r.ErrorMessage!);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.StaffManage)]
    public async Task<IActionResult> Delete(int id)
    {
        var session = _currentUser.ToUserSession()!;
        var r = await _platforms.DeleteAsync(id, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) TempData["Success"] = "تم حذف المنصة";
        else TempData["Error"] = r.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }
}
