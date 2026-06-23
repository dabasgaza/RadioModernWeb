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
public class CoverageController : Controller
{
    private readonly ICoverageService _coverage;
    private readonly ICorrespondentService _correspondents;
    private readonly IGuestService _guests;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CoverageController> _logger;

    public CoverageController(ICoverageService coverage, ICorrespondentService correspondents, IGuestService guests, ICurrentUserService currentUser, ILogger<CoverageController> logger)
    {
        _coverage = coverage; _correspondents = correspondents; _guests = guests;
        _currentUser = currentUser; _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _coverage.GetAllAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var correspondents = await _correspondents.GetAllActiveAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var guests = await _guests.GetAllActiveAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var vm = new CoverageListViewModel { Coverages = list, Correspondents = correspondents, Guests = guests };
        return View(vm);
    }

    [Authorize(Policy = AppPermissions.CoordinationManage)]
    public async Task<IActionResult> Create()
    {
        var correspondents = await _correspondents.GetAllActiveAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var guests = await _guests.GetAllActiveAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var vm = new CoverageEditViewModel
        {
            Coverage = new CoverageDto(),
            Correspondents = correspondents,
            Guests = guests
        };
        return View("Edit", vm);
    }

    [Authorize(Policy = AppPermissions.CoordinationManage)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CoverageDto model)
    {
        if (!ModelState.IsValid) { TempData["Error"] = ModelState.Values.FirstOrDefault()?.Errors.FirstOrDefault()?.ErrorMessage ?? "بيانات غير صالحة"; return RedirectToAction(nameof(Create)); }
        var session = _currentUser.ToUserSession()!;
        var r = await _coverage.CreateAsync(model, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) TempData["Success"] = "تم إضافة التغطية";
        else TempData["Error"] = r.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = AppPermissions.CoordinationManage)]
    public async Task<IActionResult> Edit(int id)
    {
        var coverage = await _coverage.GetByIdAsync(id, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (coverage == null) return NotFound();
        var correspondents = await _correspondents.GetAllActiveAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var guests = await _guests.GetAllActiveAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var vm = new CoverageEditViewModel
        {
            Coverage = coverage,
            Correspondents = correspondents,
            Guests = guests
        };
        return View(vm);
    }

    [Authorize(Policy = AppPermissions.CoordinationManage)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CoverageDto model)
    {
        if (!ModelState.IsValid) { TempData["Error"] = ModelState.Values.FirstOrDefault()?.Errors.FirstOrDefault()?.ErrorMessage ?? "بيانات غير صالحة"; return RedirectToAction(nameof(Edit), new { id }); }
        model = model with { CoverageId = id };
        var session = _currentUser.ToUserSession()!;
        var r = await _coverage.UpdateAsync(model, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) TempData["Success"] = "تم تحديث التغطية";
        else TempData["Error"] = r.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = AppPermissions.CoordinationManage)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var session = _currentUser.ToUserSession()!;
        var r = await _coverage.DeleteAsync(id, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) TempData["Success"] = "تم حذف التغطية";
        else TempData["Error"] = r.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }
}
