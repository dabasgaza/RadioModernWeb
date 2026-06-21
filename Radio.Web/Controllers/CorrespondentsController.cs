using DataAccess.Common;
using DataAccess.Validation;
using DataAccess.DTOs;
using DataAccess.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Radio.Web.Services;
using System.Threading;
using Radio.Web.ViewModels;

namespace Radio.Web.Controllers;

[Authorize]
public class CorrespondentsController : Controller
{
    private readonly ICorrespondentService _correspondents;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CorrespondentsController> _logger;

    public CorrespondentsController(ICorrespondentService correspondents, ICurrentUserService currentUser, ILogger<CorrespondentsController> logger)
    {
        _correspondents = correspondents; _currentUser = currentUser; _logger = logger;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var list = await _correspondents.GetAllActiveAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            list = list.Where(c => c.FullName?.Contains(s, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
        }
        ViewBag.Search = search ?? "";
        return View(list.OrderBy(c => c.FullName).ToList());
    }

    [Authorize(Policy = AppPermissions.CoordinationManage)]
    public IActionResult Create() => View("Edit", new CorrespondentDto(0, string.Empty, null, null));

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.CoordinationManage)]
    public async Task<IActionResult> Create(CorrespondentDto model)
    {
        var v = ValidationPipeline.ValidateCorrespondent(model);
        if (!v.IsSuccess) { ModelState.AddModelError("", v.ErrorMessage!); return View("Edit", model); }
        var session = _currentUser.ToUserSession()!;
        var r = await _correspondents.CreateAsync(model, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) { TempData["Success"] = "تم إضافة المراسل"; return RedirectToAction(nameof(Index)); }
        ModelState.AddModelError("", r.ErrorMessage!);
        return View("Edit", model);
    }

    [Authorize(Policy = AppPermissions.CoordinationManage)]
    public async Task<IActionResult> Edit(int id)
    {
        var list = await _correspondents.GetAllActiveAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var c = list.FirstOrDefault(x => x.CorrespondentId == id);
        if (c == null) return NotFound();
        return View(c);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.CoordinationManage)]
    public async Task<IActionResult> Edit(int id, CorrespondentDto model)
    {
        var v = ValidationPipeline.ValidateCorrespondent(model);
        if (!v.IsSuccess) { ModelState.AddModelError("", v.ErrorMessage!); return View(model); }
        model = model with { CorrespondentId = id };
        var session = _currentUser.ToUserSession()!;
        var r = await _correspondents.UpdateAsync(model, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) { TempData["Success"] = "تم تحديث المراسل"; return RedirectToAction(nameof(Index)); }
        ModelState.AddModelError("", r.ErrorMessage!);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.CoordinationManage)]
    public async Task<IActionResult> Delete(int id)
    {
        var session = _currentUser.ToUserSession()!;
        var r = await _correspondents.SoftDeleteAsync(id, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) TempData["Success"] = "تم حذف المراسل";
        else TempData["Error"] = r.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }

    // Coverage listing for a correspondent
    public async Task<IActionResult> Coverage(int id)
    {
        var coverages = await _correspondents.GetCoverageAsync(id, cancellationToken: HttpContext?.RequestAborted ?? default);
        return View(coverages);
    }
}
