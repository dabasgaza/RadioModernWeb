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
public class GuestsController : Controller
{
    private readonly IGuestService _guests;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<GuestsController> _logger;

    public GuestsController(IGuestService guests, ICurrentUserService currentUser, ILogger<GuestsController> logger)
    {
        _guests = guests; _currentUser = currentUser; _logger = logger;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var list = await _guests.GetAllActiveAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            list = list.Where(g => (g.FullName?.Contains(s, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                   (g.Organization?.Contains(s, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        }
        ViewBag.Search = search ?? "";
        return View(list.OrderBy(g => g.FullName).ToList());
    }

    [Authorize(Policy = AppPermissions.GuestManage)]
    public IActionResult Create() => View("Edit", new GuestDto(0, string.Empty, null, null, null, null, null));

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.GuestManage)]
    public async Task<IActionResult> Create(GuestDto model)
    {
        if (!ModelState.IsValid) return View("Edit", model);

        var session = _currentUser.ToUserSession()!;
        var r = await _guests.CreateGuestAsync(model, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) { TempData["Success"] = "تم إضافة الضيف"; return RedirectToAction(nameof(Index)); }
        ModelState.AddModelError("", r.ErrorMessage!);
        return View("Edit", model);
    }

    [Authorize(Policy = AppPermissions.GuestManage)]
    public async Task<IActionResult> Edit(int id)
    {
        var list = await _guests.GetAllActiveAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var g = list.FirstOrDefault(x => x.GuestId == id);
        if (g == null) return NotFound();
        return View(g);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.GuestManage)]
    public async Task<IActionResult> Edit(int id, GuestDto model)
    {
        if (!ModelState.IsValid) return View(model);

        model = model with { GuestId = id };
        var session = _currentUser.ToUserSession()!;
        var r = await _guests.UpdateGuestAsync(model, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) { TempData["Success"] = "تم تحديث الضيف"; return RedirectToAction(nameof(Index)); }
        ModelState.AddModelError("", r.ErrorMessage!);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.GuestManage)]
    public async Task<IActionResult> Delete(int id)
    {
        var session = _currentUser.ToUserSession()!;
        var r = await _guests.SoftDeleteGuestAsync(id, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) TempData["Success"] = "تم حذف الضيف";
        else TempData["Error"] = r.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }
}
