using DataAccess.Common;
using DataAccess.DTOs;
using DataAccess.Services;
using Domain.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Radio.Web.Services;
using System.Threading;
using Radio.Web.ViewModels;

namespace Radio.Web.Controllers;

[Authorize]
public class EpisodesController : Controller
{
    private readonly IEpisodeQueryService _query;
    private readonly IEpisodeCommandService _command;
    private readonly IExecutionService _execution;
    private readonly IPublishingQueryService _publishing;
    private readonly ICachedLookupService _lookup;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<EpisodesController> _logger;
    private readonly IValidator<EpisodeDto> _episodeValidator;

    public EpisodesController(
        IEpisodeQueryService query,
        IEpisodeCommandService command,
        IExecutionService execution,
        IPublishingQueryService publishing,
        ICachedLookupService lookup,
        ICurrentUserService currentUser,
        ILogger<EpisodesController> logger,
        IValidator<EpisodeDto> episodeValidator)
    {
        _query = query;
        _command = command;
        _execution = execution;
        _publishing = publishing;
        _lookup = lookup;
        _currentUser = currentUser;
        _logger = logger;
        _episodeValidator = episodeValidator;
    }

    // GET: /Episodes
    public async Task<IActionResult> Index(string? search, byte? status, int? programId)
    {
        try
        {
            var episodes = await _query.GetActiveEpisodesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
            var programs = await _lookup.GetProgramsAsync(cancellationToken: HttpContext?.RequestAborted ?? default);

            var filtered = episodes.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                filtered = filtered.Where(e =>
                    (e.EpisodeName?.Contains(s, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (e.ProgramName?.Contains(s, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    e.GuestItems.Any(g => g.Name?.Contains(s, StringComparison.OrdinalIgnoreCase) ?? false));
            }
            if (status.HasValue) filtered = filtered.Where(e => e.StatusId == status.Value);
            if (programId.HasValue) filtered = filtered.Where(e => e.ProgramId == programId.Value);

            var vm = new EpisodeListViewModel
            {
                Episodes = filtered.OrderByDescending(e => e.ScheduledExecutionTime ?? DateTime.MinValue).ToList(),
                Programs = programs,
                SearchTerm = search ?? "",
                StatusFilter = status,
                ProgramFilter = programId
            };
            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "فشل في تحميل قائمة الحلقات");
            return View("Error", new ErrorViewModel { ErrorMessage = ex.Message });
        }
    }

    // GET: /Episodes/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var episode = await _query.GetActiveEpisodeByIdAsync(id, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (episode == null) return NotFound();

        var publishingRecords = await _publishing.GetAllPublishingRecordsAsync(id, cancellationToken: HttpContext?.RequestAborted ?? default);
        var vm = new EpisodeDetailsViewModel
        {
            Episode = episode,
            PublishingRecords = publishingRecords
        };
        return View(vm);
    }

    // GET: /Episodes/Create
    [Authorize(Policy = AppPermissions.EpisodeManage)]
    public async Task<IActionResult> Create()
    {
        var vm = await BuildEditViewModelAsync(new EpisodeDto(0, 0, new(), new(), new(), string.Empty, null, null, null, null));
        vm.StatusText = "مجدولة";
        vm.StatusId = 0;
        return View("Edit", vm);
    }

    // POST: /Episodes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.EpisodeManage)]
    public async Task<IActionResult> Create(EpisodeEditFormModel form)
    {
        var dto = form.ToDto();
        var validation = await _episodeValidator.ValidateAsync(dto);
        if (!validation.IsValid || !ModelState.IsValid)
        {
            if (!validation.IsValid)
                foreach (var err in validation.Errors)
                    ModelState.AddModelError("", err.ErrorMessage);
            var vm = await BuildEditViewModelAsync(dto);
            return View("Edit", vm);
        }

        var session = _currentUser.ToUserSession()!;
        var result = await _command.CreateEpisodeAsync(dto, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (result.IsSuccess)
        {
            TempData["Success"] = "تم إنشاء الحلقة بنجاح";
            return RedirectToAction(nameof(Details), new { id = result.Value });
        }
        ModelState.AddModelError("", result.ErrorMessage!);
        var vm2 = await BuildEditViewModelAsync(dto);
        return View("Edit", vm2);
    }

    // GET: /Episodes/Edit/5
    [Authorize(Policy = AppPermissions.EpisodeEdit)]
    public async Task<IActionResult> Edit(int id)
    {
        var episode = await _query.GetActiveEpisodeByIdAsync(id, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (episode == null) return NotFound();

        var dto = new EpisodeDto(
            episode.EpisodeId,
            episode.ProgramId,
            episode.GuestItems.Select(g => new EpisodeGuestDto(0, g.GuestId, g.Name, g.Topic, g.HostingTime, null)).ToList(),
            episode.CorrespondentItems.Select(c => new EpisodeCorrespondentDto(c.Id, c.CorrespondentId, c.FullName, c.Topic, c.HostingTime)).ToList(),
            episode.EmployeeItems.Select(e => new EpisodeEmployeeDto(e.Id, e.EmployeeId, e.FullName, e.StaffRoleName)).ToList(),
            episode.EpisodeName ?? string.Empty,
            episode.EpisodeDescription,
            episode.ScheduledExecutionTime?.Date,
            episode.ScheduledExecutionTime?.TimeOfDay,
            episode.SpecialNotes);

        var vm = await BuildEditViewModelAsync(dto);
        vm.StatusText = episode.StatusText;
        vm.StatusId = episode.StatusId;
        return View(vm);
    }

    // POST: /Episodes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.EpisodeEdit)]
    public async Task<IActionResult> Edit(int id, EpisodeEditFormModel form)
    {
        var dto = form.ToDto();
        var validation = await _episodeValidator.ValidateAsync(dto);
        if (!validation.IsValid || !ModelState.IsValid)
        {
            if (!validation.IsValid)
                foreach (var err in validation.Errors)
                    ModelState.AddModelError("", err.ErrorMessage);
            var vm = await BuildEditViewModelAsync(dto);
            return View(vm);
        }

        var session = _currentUser.ToUserSession()!;
        var result = await _command.UpdateEpisodeAsync(dto, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (result.IsSuccess)
        {
            TempData["Success"] = "تم تحديث الحلقة بنجاح";
            return RedirectToAction(nameof(Details), new { id });
        }
        ModelState.AddModelError("", result.ErrorMessage!);
        var vm2 = await BuildEditViewModelAsync(dto);
        return View(vm2);
    }

    // POST: /Episodes/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.EpisodeDelete)]
    public async Task<IActionResult> Delete(int id)
    {
        var session = _currentUser.ToUserSession()!;
        var result = await _command.DeleteEpisodeAsync(id, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (result.IsSuccess) TempData["Success"] = "تم حذف الحلقة";
        else TempData["Error"] = result.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }

    // POST: /Episodes/Execute/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.EpisodeExecute)]
    public async Task<IActionResult> Execute(int id, string executionNotes, string issuesEncountered, int durationMinutes)
    {
        var session = _currentUser.ToUserSession()!;
        var dto = new ExecutionLogDto
        {
            EpisodeId = id,
            ExecutedByUserId = session.UserId,
            ExecutionNotes = executionNotes ?? string.Empty,
            IssuesEncountered = issuesEncountered ?? string.Empty,
            DurationMinutes = durationMinutes
        };
        var result = await _execution.LogExecutionAsync(dto, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (result.IsSuccess) TempData["Success"] = "تم تسجيل التنفيذ بنجاح";
        else TempData["Error"] = result.ErrorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /Episodes/Cancel/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.EpisodeRevert)]
    public async Task<IActionResult> Cancel(int id, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["Error"] = "يرجى إدخال سبب الإلغاء";
            return RedirectToAction(nameof(Details), new { id });
        }
        var session = _currentUser.ToUserSession()!;
        var result = await _command.CancelEpisodeAsync(id, reason, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (result.IsSuccess) TempData["Success"] = "تم إلغاء الحلقة";
        else TempData["Error"] = result.ErrorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /Episodes/Revert/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.EpisodeRevert)]
    public async Task<IActionResult> Revert(int id, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["Error"] = "يرجى إدخال سبب التراجع";
            return RedirectToAction(nameof(Details), new { id });
        }
        var session = _currentUser.ToUserSession()!;
        var result = await _command.RevertEpisodeStatusAsync(id, reason, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (result.IsSuccess) TempData["Success"] = "تم التراجع عن الحالة";
        else TempData["Error"] = result.ErrorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /Episodes/BatchDelete
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.EpisodeDelete)]
    public async Task<IActionResult> BatchDelete(int[] ids)
    {
        if (ids == null || ids.Length == 0) return RedirectToAction(nameof(Index));
        var session = _currentUser.ToUserSession()!;
        var (success, fail) = await _command.DeleteEpisodesBatchAsync(ids.ToList(), session, cancellationToken: HttpContext?.RequestAborted ?? default);
        TempData["Info"] = $"تم حذف {success} بنجاح، فشل {fail}";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Episodes/BatchCancel
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AppPermissions.EpisodeRevert)]
    public async Task<IActionResult> BatchCancel(int[] ids, string reason)
    {
        if (ids == null || ids.Length == 0) return RedirectToAction(nameof(Index));
        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["Error"] = "يرجى إدخال سبب الإلغاء";
            return RedirectToAction(nameof(Index));
        }
        var session = _currentUser.ToUserSession()!;
        var (success, fail) = await _command.CancelEpisodesBatchAsync(ids.ToList(), reason, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        TempData["Info"] = $"تم إلغاء {success} بنجاح، فشل {fail}";
        return RedirectToAction(nameof(Index));
    }

    private async Task<EpisodeEditViewModel> BuildEditViewModelAsync(EpisodeDto dto)
    {
        var programs = await _lookup.GetProgramsAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var guests = await _lookup.GetGuestsAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var correspondents = await _lookup.GetCorrespondentsAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var staffRoles = await _lookup.GetStaffRolesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var employees = await _lookup.GetEmployeesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);

        return new EpisodeEditViewModel
        {
            Episode = dto,
            Programs = programs,
            Guests = guests,
            Correspondents = correspondents,
            StaffRoles = staffRoles,
            Employees = employees
        };
    }
}
