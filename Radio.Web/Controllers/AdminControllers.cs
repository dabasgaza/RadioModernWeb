// ============================================================
// AdminControllers — الإدارة
// ============================================================
// المسؤولية: تعريف الإدارة.
// ============================================================
using DataAccess.Common;
using DataAccess.DTOs;
using DataAccess.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Radio.Web.Services;
using Radio.Web.ViewModels;

namespace Radio.Web.Controllers;

/// <summary>
/// صنف المستخدمين.
/// </summary>
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

    /// <summary>
    /// عرض قائمة المستخدمين.
    /// </summary>
    [Authorize(Policy = AppPermissions.UserView)]
    public async Task<IActionResult> Index(string? search)
    {
        var list = await _users.GetAllUsersAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            list = list.Where(u => (u.FullName?.Contains(s, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                   (u.Username?.Contains(s, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        }
        ViewBag.Search = search ?? string.Empty;
        return View(list.OrderBy(u => u.FullName).ToList());
    }

    /// <summary>
    /// إنشاء المستخدمين.
    /// </summary>
    [Authorize(Policy = AppPermissions.UserManage)]
    public async Task<IActionResult> Create()
    {
        ViewBag.Roles = await _users.GetRolesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        return View("Edit", new UserViewModel { IsActive = true });
    }

    /// <summary>
    /// إنشاء المستخدمين.
    /// </summary>
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
        if (errors.Count > 0) { foreach (var err in errors) ModelState.AddModelError(string.Empty, err); return View("Edit", model); }

        var session = _currentUser.ToUserSession()!;
        var r = await _users.CreateUserAsync(dto, model.Password ?? string.Empty, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) { TempData["Success"] = "تم إنشاء المستخدم"; return RedirectToAction(nameof(Index)); }
        ModelState.AddModelError(string.Empty, r.ErrorMessage!);
        return View("Edit", model);
    }

    /// <summary>
    /// تعديل المستخدمين.
    /// </summary>
    [Authorize(Policy = AppPermissions.UserManage)]
    public async Task<IActionResult> Edit(int id)
    {
        var list = await _users.GetAllUsersAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var u = list.FirstOrDefault(x => x.UserId == id);
        if (u == null) return NotFound();
        ViewBag.Roles = await _users.GetRolesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        return View(new UserViewModel { UserId = u.UserId, FullName = u.FullName, Username = u.Username, EmailAddress = u.EmailAddress, PhoneNumber = u.PhoneNumber, RoleId = u.RoleId, IsActive = u.IsActive });
    }

    /// <summary>
    /// تعديل المستخدمين.
    /// </summary>
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
        if (errors.Count > 0) { foreach (var err in errors) ModelState.AddModelError(string.Empty, err); return View(model); }

        var session = _currentUser.ToUserSession()!;
        var r = await _users.UpdateUserAsync(dto, string.IsNullOrWhiteSpace(model.Password) ? null : model.Password, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) { TempData["Success"] = "تم تحديث المستخدم"; return RedirectToAction(nameof(Index)); }
        ModelState.AddModelError(string.Empty, r.ErrorMessage!);
        return View(model);
    }

    /// <summary>
    /// تبديل الحالة.
    /// </summary>
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

    /// <summary>
    /// حذف المستخدمين.
    /// </summary>
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

    /// <summary>
    /// عرض تفاصيل المستخدم والصلاحيات الفعالة ومصدر كل صلاحية.
    /// </summary>
    [Authorize(Policy = AppPermissions.UserView)]
    public async Task<IActionResult> Details(int id)
    {
        var list = await _users.GetAllUsersAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var u = list.FirstOrDefault(x => x.UserId == id);
        if (u == null) return NotFound();

        var matrix = await _users.GetUserPermissionsMatrixAsync(id, cancellationToken: HttpContext?.RequestAborted ?? default);
        ViewBag.User = u;
        return View(matrix);
    }

    /// <summary>
    /// عرض وإدارة استثناءات الصلاحيات الفردية للمستخدم.
    /// </summary>
    [Authorize(Policy = AppPermissions.UserManage)]
    public async Task<IActionResult> Permissions(int id)
    {
        var list = await _users.GetAllUsersAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var u = list.FirstOrDefault(x => x.UserId == id);
        if (u == null) return NotFound();

        var matrix = await _users.GetUserPermissionsMatrixAsync(id, cancellationToken: HttpContext?.RequestAborted ?? default);
        ViewBag.User = u;
        return View(matrix);
    }

    /// <summary>
    /// تحديث استثناءات الصلاحيات الفردية للمستخدم.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.UserManage)]
    public async Task<IActionResult> UpdatePermissions(int id, List<string> grantedPermissions, List<string> deniedPermissions)
    {
        var session = _currentUser.ToUserSession()!;
        var r = await _users.UpdateUserPermissionsAsync(id, grantedPermissions, deniedPermissions, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) TempData["Success"] = "تم تحديث استثناءات الصلاحيات للمستخدم بنجاح";
        else TempData["Error"] = r.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }
}

/// <summary>
/// صنف الأدوار.
/// </summary>
[Authorize]
public class RolesController : Controller
{
    private readonly IUserService _users;
    private readonly ICurrentUserService _currentUser;

    public RolesController(IUserService users, ICurrentUserService currentUser)
    {
        _users = users; _currentUser = currentUser;
    }

    /// <summary>
    /// عرض قائمة الأدوار.
    /// </summary>
    [Authorize(Policy = AppPermissions.UserView)]
    public async Task<IActionResult> Index()
    {
        var list = await _users.GetRolesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        return View(list);
    }

    /// <summary>
    /// إنشاء الأدوار.
    /// </summary>
    [Authorize(Policy = AppPermissions.UserManage)]
    public IActionResult Create() => View("Edit", new RoleDto());

    /// <summary>
    /// إنشاء الأدوار.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.UserManage)]
    public async Task<IActionResult> Create(RoleDto model)
    {
        if (string.IsNullOrWhiteSpace(model.RoleName)) { ModelState.AddModelError("RoleName", "اسم الدور مطلوب."); return View("Edit", model); }
        var session = _currentUser.ToUserSession()!;
        var r = await _users.CreateRoleAsync(model, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) { TempData["Success"] = "تم إضافة الدور"; return RedirectToAction(nameof(Index)); }
        ModelState.AddModelError(string.Empty, r.ErrorMessage!);
        return View("Edit", model);
    }

    /// <summary>
    /// تعديل الأدوار.
    /// </summary>
    [Authorize(Policy = AppPermissions.UserManage)]
    public async Task<IActionResult> Edit(int id)
    {
        var list = await _users.GetRolesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var r = list.FirstOrDefault(x => x.RoleId == id);
        if (r == null) return NotFound();
        return View(r);
    }

    /// <summary>
    /// تعديل الأدوار.
    /// </summary>
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
        ModelState.AddModelError(string.Empty, r.ErrorMessage!);
        return View(model);
    }

    /// <summary>
    /// حذف الأدوار.
    /// </summary>
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

    /// <summary>
    /// عرض صفحة نسخ الدور.
    /// </summary>
    [Authorize(Policy = AppPermissions.UserManage)]
    public async Task<IActionResult> Clone(int id)
    {
        var list = await _users.GetRolesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var r = list.FirstOrDefault(x => x.RoleId == id);
        if (r == null) return NotFound();

        ViewBag.SourceRoleId = id;
        ViewBag.SourceRoleName = r.RoleName;
        return View(new RoleDto { RoleDescription = r.RoleDescription });
    }

    /// <summary>
    /// معالجة نسخ الدور.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.UserManage)]
    public async Task<IActionResult> Clone(int id, string newRoleName, string newDescription)
    {
        if (string.IsNullOrWhiteSpace(newRoleName))
        {
            ModelState.AddModelError(string.Empty, "اسم الدور الجديد مطلوب.");
            return await Clone(id);
        }

        var session = _currentUser.ToUserSession()!;
        var r = await _users.CloneRoleAsync(id, newRoleName, newDescription, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess)
        {
            TempData["Success"] = "تم استنساخ الدور بنجاح";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, r.ErrorMessage!);
        return await Clone(id);
    }
}

/// <summary>
/// صنف الصلاحيات.
/// </summary>
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

    /// <summary>
    /// عرض قائمة الصلاحيات.
    /// </summary>
    [Authorize(Policy = AppPermissions.UserView)]
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

    /// <summary>
    /// تحديث الصلاحيات.
    /// </summary>
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

/// <summary>
/// صنف التدقيق السجلات.
/// </summary>
[Authorize]
public class AuditLogsController : Controller
{
    private readonly IAuditLogService _auditLog;

    /// <summary>
    /// تهيئة التدقيق السجلات.
    /// </summary>
    public AuditLogsController(IAuditLogService auditLog) => _auditLog = auditLog;

    /// <summary>
    /// عرض قائمة التدقيق السجلات.
    /// </summary>
    [Authorize(Policy = AppPermissions.ViewAuditLogs)]
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
