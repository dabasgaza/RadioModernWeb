using DataAccess.Common;
using DataAccess.DTOs;
using DataAccess.Services;
using Domain.Identity;
using Domain.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Radio.Web.Security;
using Radio.Web.Services;
using System.Threading;
using Radio.Web.ViewModels;

namespace Radio.Web.Controllers;

[Authorize]
public class UsersController : Controller
{
    private readonly IUserService _users;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UsersController> _logger;
    private readonly IValidator<UserDto> _userValidator;

    public UsersController(IUserService users, ICurrentUserService currentUser, ILogger<UsersController> logger, IValidator<UserDto> userValidator)
    {
        _users = users; _currentUser = currentUser; _logger = logger; _userValidator = userValidator;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var list = await _users.GetAllUsersAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            list = list.Where(u => (u.FullName?.Contains(s, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                   (u.Username?.Contains(s, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        }
        ViewBag.Search = search ?? "";
        return View(list.OrderBy(u => u.FullName).ToList());
    }

    [Authorize(Policy = AppPermissions.UserManage)]
    public async Task<IActionResult> Create()
    {
        ViewBag.Roles = await _users.GetRolesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        return View("Edit", new UserViewModel { IsActive = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.UserManage)]
    public async Task<IActionResult> Create(UserViewModel model)
    {
        ViewBag.Roles = await _users.GetRolesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var dto = model.ToDto();
        var validation = await _userValidator.ValidateAsync(dto);
        var errors = validation.Errors.Select(e => e.ErrorMessage).ToList();
        if (model.UserId == 0 && string.IsNullOrWhiteSpace(model.Password))
            errors.Add("كلمة المرور مطلوبة للمستخدم الجديد.");
        if (!string.IsNullOrWhiteSpace(model.Password) && model.Password.Length < 6)
            errors.Add("كلمة المرور يجب أن تكون 6 أحرف على الأقل.");
        if (errors.Count > 0) { foreach (var err in errors) ModelState.AddModelError("", err); return View("Edit", model); }

        var session = _currentUser.ToUserSession()!;
        var r = await _users.CreateUserAsync(dto, model.Password ?? string.Empty, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) { TempData["Success"] = "تم إنشاء المستخدم"; return RedirectToAction(nameof(Index)); }
        ModelState.AddModelError("", r.ErrorMessage!);
        return View("Edit", model);
    }

    [Authorize(Policy = AppPermissions.UserManage)]
    public async Task<IActionResult> Edit(int id)
    {
        var list = await _users.GetAllUsersAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var u = list.FirstOrDefault(x => x.UserId == id);
        if (u == null) return NotFound();
        ViewBag.Roles = await _users.GetRolesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        return View(new UserViewModel { UserId = u.UserId, FullName = u.FullName, Username = u.Username, EmailAddress = u.EmailAddress, PhoneNumber = u.PhoneNumber, RoleId = u.RoleId, IsActive = u.IsActive });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.UserManage)]
    public async Task<IActionResult> Edit(int id, UserViewModel model)
    {
        ViewBag.Roles = await _users.GetRolesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        model.UserId = id;
        var dto = model.ToDto();
        var validation = await _userValidator.ValidateAsync(dto);
        var errors = validation.Errors.Select(e => e.ErrorMessage).ToList();
        if (!string.IsNullOrWhiteSpace(model.Password) && model.Password.Length < 6)
            errors.Add("كلمة المرور يجب أن تكون 6 أحرف على الأقل.");
        if (errors.Count > 0) { foreach (var err in errors) ModelState.AddModelError("", err); return View(model); }

        var session = _currentUser.ToUserSession()!;
        var r = await _users.UpdateUserAsync(dto, string.IsNullOrWhiteSpace(model.Password) ? null : model.Password, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) { TempData["Success"] = "تم تحديث المستخدم"; return RedirectToAction(nameof(Index)); }
        ModelState.AddModelError("", r.ErrorMessage!);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.UserManage)]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var list = await _users.GetAllUsersAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var u = list.FirstOrDefault(x => x.UserId == id);
        if (u == null) return NotFound();
        var session = _currentUser.ToUserSession()!;
        var r = await _users.ToggleUserStatusAsync(id, !u.IsActive, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) TempData["Success"] = "تم تحديث حالة المستخدم";
        else TempData["Error"] = r.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.UserManage)]
    public async Task<IActionResult> Delete(int id)
    {
        var session = _currentUser.ToUserSession()!;
        var r = await _users.DeleteUserAsync(id, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) TempData["Success"] = "تم حذف المستخدم";
        else TempData["Error"] = r.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }
}

[Authorize]
public class RolesController : Controller
{
    private readonly IUserService _users;
    private readonly ICurrentUserService _currentUser;

    public RolesController(IUserService users, ICurrentUserService currentUser)
    {
        _users = users; _currentUser = currentUser;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _users.GetRolesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        return View(list);
    }

    [Authorize(Policy = AppPermissions.UserManage)]
    public IActionResult Create() => View("Edit", new RoleDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.UserManage)]
    public async Task<IActionResult> Create(RoleDto model)
    {
        if (string.IsNullOrWhiteSpace(model.RoleName)) { ModelState.AddModelError("RoleName", "اسم الدور مطلوب."); return View("Edit", model); }
        var session = _currentUser.ToUserSession()!;
        var r = await _users.CreateRoleAsync(model, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) { TempData["Success"] = "تم إضافة الدور"; return RedirectToAction(nameof(Index)); }
        ModelState.AddModelError("", r.ErrorMessage!);
        return View("Edit", model);
    }

    [Authorize(Policy = AppPermissions.UserManage)]
    public async Task<IActionResult> Edit(int id)
    {
        var list = await _users.GetRolesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var r = list.FirstOrDefault(x => x.RoleId == id);
        if (r == null) return NotFound();
        return View(r);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.UserManage)]
    public async Task<IActionResult> Edit(int id, RoleDto model)
    {
        if (string.IsNullOrWhiteSpace(model.RoleName)) { ModelState.AddModelError("RoleName", "اسم الدور مطلوب."); return View(model); }
        model.RoleId = id;
        var session = _currentUser.ToUserSession()!;
        var r = await _users.UpdateRoleAsync(model, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) { TempData["Success"] = "تم تحديث الدور"; return RedirectToAction(nameof(Index)); }
        ModelState.AddModelError("", r.ErrorMessage!);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.UserManage)]
    public async Task<IActionResult> Delete(int id)
    {
        var session = _currentUser.ToUserSession()!;
        var r = await _users.DeleteRoleAsync(id, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) TempData["Success"] = "تم حذف الدور";
        else TempData["Error"] = r.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }
}

[Authorize]
public class PermissionsController : Controller
{
    private readonly IUserService _users;
    private readonly ICurrentUserService _currentUser;

    public PermissionsController(IUserService users, ICurrentUserService currentUser)
    {
        _users = users;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index(int? roleId)
    {
        var roles = await _users.GetRolesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var vm = new PermissionMatrixViewModel { Roles = roles, SelectedRoleId = roleId ?? 0 };

        if (roleId.HasValue && roleId > 0)
        {
            vm.Permissions = await _users.GetPermissionsMatrixAsync(roleId.Value, cancellationToken: HttpContext?.RequestAborted ?? default);
            vm.SelectedRoleName = roles.FirstOrDefault(r => r.RoleId == roleId.Value)?.RoleName;
        }
        else if (roles.Any())
        {
            vm.SelectedRoleId = roles.First().RoleId;
            vm.Permissions = await _users.GetPermissionsMatrixAsync(vm.SelectedRoleId, cancellationToken: HttpContext?.RequestAborted ?? default);
            vm.SelectedRoleName = roles.First().RoleName;
        }
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.UserManage)]
    public async Task<IActionResult> Update(int roleId, int[] selectedPermissionIds)
    {
        var session = _currentUser.ToUserSession()!;
        var r = await _users.UpdateRolePermissionsAsync(roleId, selectedPermissionIds.ToList(), session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) TempData["Success"] = "تم تحديث الصلاحيات";
        else TempData["Error"] = r.ErrorMessage;
        return RedirectToAction(nameof(Index), new { roleId });
    }
}

[Authorize]
public class AuditLogsController : Controller
{
    private readonly IAuditLogService _auditLog;

    public AuditLogsController(IAuditLogService auditLog) => _auditLog = auditLog;

    public async Task<IActionResult> Index(string? table, string? action, DateTime? fromDate, DateTime? toDate)
    {
        var r = await _auditLog.GetFilteredAuditLogsAsync(
            string.IsNullOrWhiteSpace(table) ? null : table,
            null,
            string.IsNullOrWhiteSpace(action) ? null : action,
            fromDate, toDate, cancellationToken: HttpContext?.RequestAborted ?? default);
        var list = r.IsSuccess ? r.Value ?? new() : new();

        ViewBag.Tables = new[] { "Episodes", "Programs", "Guests", "Correspondents", "Users", "Roles", "Permissions" };
        ViewBag.Actions = new[] { "ADDED", "MODIFIED", "DELETED", "SOFT_DELETED" };
        ViewBag.SelectedTable = table;
        ViewBag.SelectedAction = action;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        return View(list);
    }
}
