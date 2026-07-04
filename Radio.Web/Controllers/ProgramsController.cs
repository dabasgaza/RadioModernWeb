// ============================================================
// ProgramsController — البرامج
// ============================================================
// المسؤولية: تعريف البرامج.
// ============================================================
using DataAccess.Common;
using DataAccess.DTOs;
using DataAccess.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Radio.Web.Services;
using Radio.Web.ViewModels;

namespace Radio.Web.Controllers;

/// <summary>
/// صنف البرامج.
/// </summary>
[Authorize]
public class ProgramsController : Controller
{
    private readonly IProgramService _programs;
    private readonly IEpisodeService _episodes;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ProgramsController> _logger;

    public ProgramsController(
        IProgramService programs,
        IEpisodeService episodes,
        ICurrentUserService currentUser,
        ILogger<ProgramsController> logger)
    {
        _programs = programs;
        _episodes = episodes;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// عرض قائمة البرامج.
    /// </summary>
    public async Task<IActionResult> Index(string? search)
    {
        try
        {
            var programs = await _programs.GetAllActiveAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                programs = programs.Where(p =>
                    (p.ProgramName?.Contains(s, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.Category?.Contains(s, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
            }
            ViewBag.Search = search ?? string.Empty;

            var episodes = await _episodes.GetActiveEpisodesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
            var model = programs.Select(p => new ProgramViewModel
            {
                Program = p,
                EpisodeCount = episodes.Count(e => e.ProgramName == p.ProgramName)
            }).OrderBy(x => x.Program.ProgramName).ToList();

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "فشل في تحميل البرامج");
            return View("Error", new ErrorViewModel { ErrorMessage = ex.Message });
        }
    }


    /// <summary>
    /// إنشاء البرامج.
    /// </summary>
    [Authorize(Policy = AppPermissions.ProgramManage)]
    public IActionResult Create() => View("Edit", new ProgramDto(0, string.Empty, null, null));

    /// <summary>
    /// إنشاء البرامج.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.ProgramManage)]
    public async Task<IActionResult> Create(ProgramDto model)
    {
        if (string.IsNullOrWhiteSpace(model.ProgramName))
            ModelState.AddModelError("ProgramName", "اسم البرنامج مطلوب.");

        if (!ModelState.IsValid) return View("Edit", model);

        var session = _currentUser.ToUserSession()!;
        var result = await _programs.CreateProgramAsync(model, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (result.IsSuccess)
        {
            TempData["Success"] = "تم إنشاء البرنامج بنجاح";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError(string.Empty, result.ErrorMessage!);
        return View("Edit", model);
    }

    /// <summary>
    /// تعديل البرامج.
    /// </summary>
    [Authorize(Policy = AppPermissions.ProgramManage)]
    public async Task<IActionResult> Edit(int id)
    {
        var programs = await _programs.GetAllActiveAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var program = programs.FirstOrDefault(p => p.ProgramId == id);
        if (program == null) return NotFound();
        return View(program);
    }

    /// <summary>
    /// تعديل البرامج.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.ProgramManage)]
    public async Task<IActionResult> Edit(int id, ProgramDto model)
    {
        if (string.IsNullOrWhiteSpace(model.ProgramName))
            ModelState.AddModelError("ProgramName", "اسم البرنامج مطلوب.");

        if (!ModelState.IsValid) return View(model);

        model = model with { ProgramId = id };
        var session = _currentUser.ToUserSession()!;
        var result = await _programs.UpdateProgramAsync(model, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (result.IsSuccess)
        {
            TempData["Success"] = "تم تحديث البرنامج";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError(string.Empty, result.ErrorMessage!);
        return View(model);
    }

    /// <summary>
    /// حذف البرامج.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.ProgramManage)]
    public async Task<IActionResult> Delete(int id)
    {
        var session = _currentUser.ToUserSession()!;
        var result = await _programs.SoftDeleteAsync(id, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (result.IsSuccess) TempData["Success"] = "تم حذف البرنامج";
        else TempData["Error"] = result.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }
}
