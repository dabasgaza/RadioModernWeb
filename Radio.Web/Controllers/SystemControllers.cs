using System.IO;
using DataAccess.Common;
using DataAccess.DTOs;
using DataAccess.Services;
using Domain.Identity;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Radio.Web.Security;
using Radio.Web.Services;
using Radio.Web.ViewModels;

namespace Radio.Web.Controllers;

[Authorize]
public class PublishingController : Controller
{
    private readonly IPublishingQueryService _query;
    private readonly IPublishingCommandService _command;
    private readonly IEpisodeQueryService _episodes;
    private readonly ICurrentUserService _currentUser;
    private readonly ICachedLookupService _lookup;
    private readonly ILogger<PublishingController> _logger;

    public PublishingController(IPublishingQueryService query, IPublishingCommandService command, IEpisodeQueryService episodes, ICurrentUserService currentUser, ILogger<PublishingController> logger, ICachedLookupService? lookup = null)
    {
        _query = query; _command = command; _episodes = episodes; _currentUser = currentUser; _logger = logger; _lookup = lookup!;
    }

    public async Task<IActionResult> Index(string? search = null, string? type = null, int? programId = null, int? episodeId = null)
    {
        var list = await _query.GetAllPublishingRecordsAsync(episodeId, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (!string.IsNullOrWhiteSpace(type))
        {
            list = list.Where(r => r.RecordType == type).ToList();
        }
        if (programId.HasValue)
        {
            list = list.Where(r => r.ProgramId == programId.Value).ToList();
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            list = list.Where(r => (r.EpisodeName?.Contains(s, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                   (r.Summary?.Contains(s, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        }
        ViewBag.Search = search ?? "";
        ViewBag.TypeFilter = type ?? "";
        ViewBag.ProgramFilter = programId;
        ViewBag.EpisodeFilter = episodeId;
        ViewBag.Programs = _lookup != null ? await _lookup.GetProgramsAsync(cancellationToken: HttpContext?.RequestAborted ?? default) : new List<ProgramDto>();
        return View(list.OrderByDescending(r => r.RecordDate).ToList());
    }

    [Authorize(Policy = AppPermissions.EpisodePublish)]
    public async Task<IActionResult> LogSocial(int id)
    {
        var platforms = await _query.GetAllPlatformsAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var episode = await _episodes.GetActiveEpisodeByIdAsync(id, cancellationToken: HttpContext?.RequestAborted ?? default);
        var guests = await _episodes.GetEpisodeGuestsAsync(id, cancellationToken: HttpContext?.RequestAborted ?? default);
        var vm = new SocialPublishingViewModel { Episode = episode, EpisodeGuests = guests, Platforms = platforms };
        return View(vm);
    }

    [Authorize(Policy = AppPermissions.EpisodePublish)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogSocial(int id, SocialPublishingFormModel form)
    {
        var session = _currentUser.ToUserSession()!;
        var logs = form.GuestLogs.Select(g => new SocialMediaPublishingLogDto(
            0, g.EpisodeGuestId, id, g.ClipTitle,
            g.DurationMinutes.HasValue ? TimeSpan.FromMinutes(g.DurationMinutes.Value) : null,
            g.MediaType,
            g.Platforms.Select(p => new PlatformPublishDto(p.PlatformId, "", p.Url)).ToList()
        )).ToList();

        var r = await _command.LogSocialPublishingAsync(id, logs, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) { TempData["Success"] = "تم تسجيل النشر الرقمي"; return RedirectToAction(nameof(Index)); }
        TempData["Error"] = r.ErrorMessage;
        return RedirectToAction(nameof(LogSocial), new { id });
    }

    [Authorize(Policy = AppPermissions.EpisodePublish)]
    public async Task<IActionResult> Edit(int id)
    {
        var log = await _query.GetSocialPublishingLogByIdAsync(id, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (log is null) return NotFound();

        var platforms = await _query.GetAllPlatformsAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var episode = await _episodes.GetActiveEpisodeByIdAsync(log.EpisodeId, cancellationToken: HttpContext?.RequestAborted ?? default);
        var episodeRecords = await _query.GetAllPublishingRecordsAsync(log.EpisodeId, cancellationToken: HttpContext?.RequestAborted ?? default);
        var episodeSocialLogs = await _query.GetEpisodeSocialLogsAsync(log.EpisodeId, cancellationToken: HttpContext?.RequestAborted ?? default) ?? [];
        var guests = await _episodes.GetEpisodeGuestsAsync(log.EpisodeId, cancellationToken: HttpContext?.RequestAborted ?? default) ?? [];

        var guestLookup = guests.ToDictionary(g => g.EpisodeGuestId, g => g.FullName);

        var vm = new SocialPublishingEditViewModel
        {
            Log = log,
            Episode = episode,
            Platforms = platforms,
            EpisodePublishingRecords = episodeRecords,
            GuestLogs = episodeSocialLogs.Select(sl => new GuestSocialLogFormItem
            {
                LogId = sl.LogId,
                EpisodeGuestId = sl.EpisodeGuestId,
                EpisodeId = sl.EpisodeId,
                ClipTitle = sl.ClipTitle ?? "",
                DurationMinutes = (int?)(sl.Duration?.TotalMinutes),
                MediaType = sl.MediaType,
                GuestName = guestLookup.GetValueOrDefault(sl.EpisodeGuestId, "ضيف غير معروف"),
                Platforms = sl.Platforms.Select(p => new PlatformUrlFormItem
                {
                    PlatformId = p.PlatformId,
                    Url = p.Url
                }).ToList()
            }).ToList()
        };
        return View(vm);
    }

    [Authorize(Policy = AppPermissions.EpisodePublish)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SocialPublishingFormModel form)
    {
        var validLogs = form.GuestLogs?.Where(g => g.LogId > 0).ToList();
        if (validLogs == null || validLogs.Count == 0)
            return BadRequest();

        var session = _currentUser.ToUserSession()!;
        var errors = new List<string>();

        foreach (var guestLog in validLogs)
        {
            var dto = new SocialMediaPublishingLogDto(
                guestLog.LogId, guestLog.EpisodeGuestId, guestLog.EpisodeId, guestLog.ClipTitle,
                guestLog.DurationMinutes.HasValue ? TimeSpan.FromMinutes(guestLog.DurationMinutes.Value) : null,
                guestLog.MediaType,
                guestLog.Platforms.Select(p => new PlatformPublishDto(p.PlatformId, "", p.Url)).ToList()
            );

            var r = await _command.UpdateSocialPublishingLogAsync(dto, session, cancellationToken: HttpContext?.RequestAborted ?? default);
            if (!r.IsSuccess) errors.Add(r.ErrorMessage!);
        }

        if (errors.Count == 0)
        {
            TempData["Success"] = "تم تحديث سجلات النشر الرقمي";
            return RedirectToAction(nameof(Index));
        }

        TempData["Error"] = string.Join(" | ", errors);
        return RedirectToAction(nameof(Edit), new { id });
    }

    [Authorize(Policy = AppPermissions.EpisodePublish)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var session = _currentUser.ToUserSession()!;
        var r = await _command.DeleteSocialPublishingLogAsync(id, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) TempData["Success"] = "تم حذف سجل النشر الرقمي";
        else TempData["Error"] = r.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }
}

[Authorize]
public class ExecutionLogsController : Controller
{
    private readonly IPublishingQueryService _publishing;
    private readonly IEpisodeQueryService _episodes;

    public ExecutionLogsController(IPublishingQueryService publishing, IEpisodeQueryService episodes)
    {
        _publishing = publishing; _episodes = episodes;
    }

    public async Task<IActionResult> Index()
    {
        var records = await _publishing.GetAllPublishingRecordsAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var executions = records.Where(r => r.RecordType == "Execution").ToList();
        return View(executions);
    }
}

[Authorize]
public class WebsitePublishingController : Controller
{
    private readonly IPublishingQueryService _query;
    private readonly IPublishingCommandService _command;
    private readonly IEpisodeQueryService _episodes;
    private readonly ICurrentUserService _currentUser;

    public WebsitePublishingController(IPublishingQueryService query, IPublishingCommandService command, IEpisodeQueryService episodes, ICurrentUserService currentUser)
    {
        _query = query; _command = command; _episodes = episodes; _currentUser = currentUser;
    }

    public async Task<IActionResult> Index()
    {
        var records = await _query.GetAllPublishingRecordsAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var websites = records.Where(r => r.RecordType == "Website").ToList();
        return View(websites);
    }

    [Authorize(Policy = AppPermissions.EpisodeWebPublish)]
    public async Task<IActionResult> Publish(int id)
    {
        var episode = await _episodes.GetActiveEpisodeByIdAsync(id, cancellationToken: HttpContext?.RequestAborted ?? default);
        return View(episode);
    }

    [Authorize(Policy = AppPermissions.EpisodeWebPublish)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(int id, string title, MediaType mediaType, string notes)
    {
        var session = _currentUser.ToUserSession()!;
        var r = await _command.LogWebsitePublishingAsync(id, title, mediaType, notes, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) { TempData["Success"] = "تم تسجيل نشر الموقع"; return RedirectToAction(nameof(Index)); }
        TempData["Error"] = r.ErrorMessage;
        return RedirectToAction(nameof(Publish), new { id });
    }

    [Authorize(Policy = AppPermissions.EpisodeWebPublish)]
    public async Task<IActionResult> Edit(int id)
    {
        var log = await _query.GetWebsitePublishingLogByIdAsync(id, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (log is null) return NotFound();

        var episode = await _episodes.GetActiveEpisodeByIdAsync(log.EpisodeId, cancellationToken: HttpContext?.RequestAborted ?? default);
        var episodeRecords = await _query.GetAllPublishingRecordsAsync(log.EpisodeId, cancellationToken: HttpContext?.RequestAborted ?? default);
        var vm = new WebsitePublishEditViewModel
        {
            Log = log,
            Episode = episode,
            EpisodePublishingRecords = episodeRecords
        };
        return View(vm);
    }

    [Authorize(Policy = AppPermissions.EpisodeWebPublish)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string title, string mediaType, string notes)
    {
        var session = _currentUser.ToUserSession()!;
        var dto = new WebsitePublishingLogDto(id, 0, mediaType, title, notes, default);
        var r = await _command.UpdateWebsitePublishingLogAsync(dto, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) { TempData["Success"] = "تم تحديث سجل نشر الموقع"; return RedirectToAction(nameof(Index)); }
        TempData["Error"] = r.ErrorMessage;
        return RedirectToAction(nameof(Edit), new { id });
    }

    [Authorize(Policy = AppPermissions.EpisodeWebPublish)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var session = _currentUser.ToUserSession()!;
        var r = await _command.DeleteWebsitePublishingLogAsync(id, session, cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) TempData["Success"] = "تم حذف سجل نشر الموقع";
        else TempData["Error"] = r.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }
}

[Authorize]
public class ReportsController : Controller
{
    private readonly IReportsService _reports;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IReportsService reports, ILogger<ReportsController> logger)
    {
        _reports = reports; _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var today = await _reports.GetTodayEpisodesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var stats = await _reports.GetEpisodeStatusStatsAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var programs = await _reports.GetMostActiveProgramsAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var guests = await _reports.GetTopGuestsAsync(20, cancellationToken: HttpContext?.RequestAborted ?? default);
        var cancelled = await _reports.GetCancelledEpisodesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);

        var vm = new ReportsViewModel
        {
            TodayEpisodes = today,
            StatusStats = stats,
            TopPrograms = programs,
            TopGuests = guests,
            CancelledEpisodes = cancelled
        };
        return View(vm);
    }

    public async Task<IActionResult> ByDateRange(DateTime? from, DateTime? to)
    {
        from ??= DateTime.UtcNow.AddDays(-30);
        to ??= DateTime.UtcNow;
        var list = await _reports.GetEpisodesByDateRangeAsync(from.Value, to.Value, cancellationToken: HttpContext?.RequestAborted ?? default);
        ViewBag.From = from.Value.ToString("yyyy-MM-dd");
        ViewBag.To = to.Value.ToString("yyyy-MM-dd");
        return View(list);
    }

    public async Task<IActionResult> Cancelled()
    {
        var list = await _reports.GetCancelledEpisodesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        return View(list);
    }
}

[Authorize]
public class DatabaseController : Controller
{
    private readonly IDatabaseManagementService _db;
    private readonly IIdentitySynchronizer _sync;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDbContextFactory<BroadcastWorkflowDBContext> _ctxFactory;
    private readonly ILogger<DatabaseController> _logger;
    private readonly IConfiguration _configuration;

    public DatabaseController(
        IDatabaseManagementService db,
        IIdentitySynchronizer sync,
        UserManager<ApplicationUser> userManager,
        IDbContextFactory<BroadcastWorkflowDBContext> ctxFactory,
        ILogger<DatabaseController> logger,
        IConfiguration configuration)
    {
        _db = db; _sync = sync; _userManager = userManager; _ctxFactory = ctxFactory; _logger = logger;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index()
    {
        var ct = HttpContext?.RequestAborted ?? default;
        var dashboard = await _db.GetDatabaseDashboardAsync(ct);
        var logs = await _db.GetBackupHistoryAsync(ct);
        var vm = new DatabaseDashboardViewModel
        {
            DatabaseSizeBytes = dashboard?.DatabaseSizeBytes ?? 0,
            DatabaseLogSizeBytes = dashboard?.DatabaseLogSizeBytes ?? 0,
            LastBackupAt = dashboard?.LastBackupAt,
            LastBackupSizeBytes = dashboard?.LastBackupSizeBytes ?? 0,
            TotalBackups = dashboard?.TotalBackups ?? 0,
            SuccessRate = dashboard?.SuccessRate ?? 100,
            BackupsThisMonth = dashboard?.BackupsThisMonth ?? 0,
            ActiveConnections = dashboard?.ActiveConnections ?? 0,
            IsAutoBackupEnabled = dashboard?.IsAutoBackupEnabled ?? false,
            IsCloudSyncEnabled = dashboard?.IsCloudSyncEnabled ?? false,
            RetentionDays = dashboard?.RetentionDays ?? 30,
            BackupFolderSizeBytes = dashboard?.BackupFolderSizeBytes ?? 0,
            FailureCount = dashboard?.FailureCount ?? 0,
            DatabaseName = dashboard?.DatabaseName ?? "",
            BackupLogs = logs.IsSuccess ? logs.Value ?? new() : new()
        };
        return View(vm);
    }

    [Authorize(Policy = AppPermissions.DatabaseManage)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Backup(string? backupFolder)
    {
        var ct = HttpContext?.RequestAborted ?? default;
        var r = await _db.BackupDatabaseAsync(
            string.IsNullOrWhiteSpace(backupFolder) ? null : backupFolder.Trim(),
            ct);
        if (r.IsSuccess) TempData["Success"] = "تم إنشاء النسخة الاحتياطية";
        else TempData["Error"] = r.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = AppPermissions.DatabaseManage)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(IFormFile? backupFile)
    {
        if (backupFile == null || backupFile.Length == 0)
        {
            TempData["Error"] = "يرجى اختيار ملف .bak للاستعادة";
            return RedirectToAction(nameof(Index));
        }

        var tempDir = Path.Combine(Path.GetTempPath(), "RadioRestore");
        Directory.CreateDirectory(tempDir);
        var tempPath = Path.Combine(tempDir, backupFile.FileName);

        await using (var stream = new FileStream(tempPath, FileMode.Create))
        {
            await backupFile.CopyToAsync(stream);
        }

        var r = await _db.RestoreDatabaseAsync(tempPath, HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) TempData["Success"] = "تمت استعادة قاعدة البيانات بنجاح";
        else TempData["Error"] = r.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = AppPermissions.DatabaseManage)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreFromLog(int id)
    {
        var ct = HttpContext?.RequestAborted ?? default;
        using var ctx = await _ctxFactory.CreateDbContextAsync(ct);
        var log = await ctx.DatabaseBackupLogs.FindAsync(new object[] { id }, ct);
        if (log == null || string.IsNullOrEmpty(log.BackupPath))
        {
            TempData["Error"] = "السجل غير موجود أو مسار الملف غير متوفر";
            return RedirectToAction(nameof(Index));
        }
        if (!System.IO.File.Exists(log.BackupPath))
        {
            TempData["Error"] = "ملف النسخة غير موجود على الخادم";
            return RedirectToAction(nameof(Index));
        }
        var r = await _db.RestoreDatabaseAsync(log.BackupPath, ct);
        if (r.IsSuccess) TempData["Success"] = "تمت استعادة قاعدة البيانات من النسخة المحددة";
        else TempData["Error"] = r.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = AppPermissions.DatabaseManage)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Initialize()
    {
        var r = await _db.InitializeDatabaseAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        if (r.IsSuccess) TempData["Success"] = "تمت التهيئة";
        else TempData["Error"] = r.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = AppPermissions.DatabaseManage)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reset()
    {
        var ct = HttpContext?.RequestAborted ?? default;
        var r = await _db.ResetDatabaseAsync(ct);
        if (r.IsSuccess) TempData["Success"] = "تمت إعادة تعيين قاعدة البيانات بنجاح";
        else TempData["Error"] = r.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = AppPermissions.DatabaseManage)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CloudSync(int? logId, string? direct)
    {
        var ct = HttpContext?.RequestAborted ?? default;
        string? backupPath = null;

        if (logId.HasValue)
        {
            using var ctx = await _ctxFactory.CreateDbContextAsync(ct);
            var log = await ctx.DatabaseBackupLogs.FindAsync(new object[] { logId.Value }, ct);
            if (log == null) { TempData["Error"] = "السجل غير موجود"; return RedirectToAction(nameof(Index)); }
            backupPath = log.BackupPath;
        }
        else
        {
            using var ctx = await _ctxFactory.CreateDbContextAsync(ct);
            var latest = await ctx.DatabaseBackupLogs
                .Where(x => x.Status == "Success")
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(ct);
            if (latest == null) { TempData["Error"] = "لا توجد نسخ احتياطية ناجحة للمزامنة"; return RedirectToAction(nameof(Index)); }
            backupPath = latest.BackupPath;
        }

        var result = await _db.CloudSyncBackupAsync(backupPath, ct);
        if (result.IsSuccess) TempData["Success"] = "تمت المزامنة السحابية";
        else TempData["Error"] = result.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = AppPermissions.DatabaseManage)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RunRetention(int retentionDays)
    {
        var ct = HttpContext?.RequestAborted ?? default;
        if (retentionDays < 1) retentionDays = 30;
        var r = await _db.RunRetentionPolicyAsync(retentionDays, ct);
        if (r.IsSuccess) TempData["Success"] = $"تم تطبيق سياسة الاحتفاظ: حذف النسخ الأقدم من {retentionDays} يوم";
        else TempData["Error"] = r.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = AppPermissions.DatabaseManage)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBackupLog(int id)
    {
        var ct = HttpContext?.RequestAborted ?? default;
        try
        {
            using var ctx = await _ctxFactory.CreateDbContextAsync(ct);
            var log = await ctx.DatabaseBackupLogs.FindAsync(new object[] { id }, ct);
            if (log != null)
            {
                log.IsActive = false;
                log.UpdatedAt = DateTime.UtcNow;
                await ctx.SaveChangesAsync(ct);
                TempData["Success"] = "تم حذف السجل";
            }
            else TempData["Error"] = "السجل غير موجود";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete backup log failed");
            TempData["Error"] = "حدث خطأ أثناء حذف السجل";
        }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = AppPermissions.DatabaseManage)]
    public async Task<IActionResult> DownloadBackup(int id)
    {
        var ct = HttpContext?.RequestAborted ?? default;
        try
        {
            using var ctx = await _ctxFactory.CreateDbContextAsync(ct);
            var log = await ctx.DatabaseBackupLogs.FindAsync(new object[] { id }, ct);
            if (log == null || string.IsNullOrEmpty(log.BackupPath) || !System.IO.File.Exists(log.BackupPath))
            {
                TempData["Error"] = "ملف النسخة غير موجود";
                return RedirectToAction(nameof(Index));
            }
            var bytes = await System.IO.File.ReadAllBytesAsync(log.BackupPath, ct);
            return File(bytes, "application/octet-stream", Path.GetFileName(log.BackupPath));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Download backup failed");
            TempData["Error"] = "حدث خطأ أثناء تحميل الملف";
            return RedirectToAction(nameof(Index));
        }
    }

    [Authorize(Policy = AppPermissions.DatabaseManage)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SeedIdentity()
    {
        var defaultPassword = _configuration["Admin:InitialPassword"]
            ?? throw new InvalidOperationException("Admin:InitialPassword must be set in configuration");

        if (defaultPassword == "Admin@123")
        {
            TempData["Error"] = "كلمة المرور الافتراضية Admin@123 غير مسموح بها في الإنتاج";
            return RedirectToAction(nameof(Index));
        }

        await using var context = await _ctxFactory.CreateDbContextAsync();

        var domainUsers = await context.Users
            .Where(u => u.IsActive)
            .ToListAsync();

        var synced = 0;
        var fixedDomain = 0;
        foreach (var du in domainUsers)
        {
            try
            {
                var existing = await _userManager.FindByNameAsync(du.Username);
                if (existing != null)
                {
                    if (existing.DomainUserId == 0)
                    {
                        existing.DomainUserId = du.UserId;
                        existing.DomainRoleId = du.RoleId;
                        existing.Email = du.EmailAddress;
                        existing.FullName = du.FullName;
                        await _userManager.UpdateAsync(existing);
                        fixedDomain++;
                    }
                    continue;
                }

                await _sync.CreateUserAsync(
                    du.Username, defaultPassword, du.FullName,
                    du.EmailAddress, du.PhoneNumber, du.RoleId, cancellationToken: HttpContext?.RequestAborted ?? default);
                synced++;

                var roleName = await context.Roles
                    .Where(r => r.RoleId == du.RoleId)
                    .Select(r => r.RoleName)
                    .FirstOrDefaultAsync();
                if (roleName != null)
                {
                    var created = await _userManager.FindByNameAsync(du.Username);
                    if (created != null)
                        await _userManager.AddToRoleAsync(created, roleName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to sync user {Username}", du.Username);
            }
        }

        TempData["Success"] = $"تمت مزامنة {synced} مستخدم/مستخدمين إلى Identity، وتم إصلاح {fixedDomain}.";
        return RedirectToAction(nameof(Index));
    }
}

[Authorize]
public class DiagnosticsController : Controller
{
    private readonly ISystemDiagnosticsService _diag;

    public DiagnosticsController(ISystemDiagnosticsService diag) => _diag = diag;

    public async Task<IActionResult> Index()
    {
        var summary = await _diag.GetSummaryAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
        var logs = await _diag.GetLogsAsync(count: 100, cancellationToken: HttpContext?.RequestAborted ?? default);
        var vm = new DiagnosticsViewModel
        {
            Summary = summary.IsSuccess && summary.Value is not null ? summary.Value : new DiagnosticsSummaryDto(),
            Logs = logs.IsSuccess ? logs.Value ?? new() : new()
        };
        return View(vm);
    }
}
