// ============================================================
// GuestService — الضيف
// ============================================================
// المسؤولية: تعريف الضيف.
// ============================================================
using DataAccess.Common;
using DataAccess.DTOs;
using Domain.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Services;

/// <summary>
/// واجهة I الضيف استعلام.
/// </summary>
public interface IGuestQueryService
{
    Task<List<GuestDto>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// واجهة I الضيف أمر.
/// </summary>
public interface IGuestCommandService
{
    Task<Result> CreateGuestAsync(GuestDto dto, UserSession session, CancellationToken cancellationToken = default);
    Task<Result> UpdateGuestAsync(GuestDto dto, UserSession session, CancellationToken cancellationToken = default);
    Task<Result> SoftDeleteGuestAsync(int guestId, UserSession session, CancellationToken cancellationToken = default);
}

/// <summary>
/// واجهة I الضيف.
/// </summary>
public interface IGuestService : IGuestQueryService, IGuestCommandService { }

// ✨ استخدام Primary Constructor وإزالة IAuditService
/// <summary>
/// صنف الضيف.
/// </summary>
public class GuestService : IGuestService
{
    private readonly IDbContextFactory<BroadcastWorkflowDBContext> _contextFactory;
    private readonly ICachedLookupService _cachedLookup;
    private readonly ILogger<GuestService> _logger;
    private readonly IValidator<GuestDto> _guestValidator;

    public GuestService(
        IDbContextFactory<BroadcastWorkflowDBContext> contextFactory,
        ICachedLookupService cachedLookup,
        ILogger<GuestService> logger,
        IValidator<GuestDto> guestValidator)
    {
        _contextFactory = contextFactory;
        _cachedLookup = cachedLookup;
        _logger = logger;
        _guestValidator = guestValidator;
    }
    // ──────────────────────────────────────────────────────────────
    // Compiled Query — تقليل وقت ترجمة LINQ في المسارات الساخنة
    // يُستدعى عند كل فتح لنموذج الحلقات أو شاشة الضيوف
    // ──────────────────────────────────────────────────────────────
    private static readonly Func<BroadcastWorkflowDBContext, IAsyncEnumerable<GuestDto>> s_compiledGetAllActive =
        EF.CompileAsyncQuery((BroadcastWorkflowDBContext context) =>
            context.Guests
                .AsNoTracking()
                .Where(g => g.IsActive)
                .Select(g => new GuestDto
                (
                    g.GuestId,
                    g.FullName,
                    g.Organization,
                    g.PhoneNumber,
                    g.EmailAddress,
                    string.Empty,
                    string.Empty)));

    /// <summary>
    /// استرجاع النشط Async.
    /// </summary>
    public async Task<List<GuestDto>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // ✨ استخدام Compiled Query — يُجمع مرة واحدة فقط بدلاً من كل استدعاء
        var result = new List<GuestDto>();
        await foreach (var dto in s_compiledGetAllActive(context))
            result.Add(dto);
        return result;
    }

    /// <summary>
    /// إنشاء الضيف Async.
    /// </summary>
    public async Task<Result> CreateGuestAsync(GuestDto dto, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.GuestManage);
        if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

        var validation = await _guestValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return Result.Fail(string.Join(Environment.NewLine, validation.Errors.Select(e => e.ErrorMessage)));

        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var guest = new Guest
            {
                FullName = dto.FullName,
                Organization = dto.Organization,
                PhoneNumber = dto.PhoneNumber,
                EmailAddress = dto.EmailAddress
            };

            context.Guests.Add(guest);
            await context.SaveChangesAsync(cancellationToken);

            // ✨ إبطال كاش الضيوف بعد الإضافة
            await _cachedLookup.InvalidateByEntity("Guest");

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Guest: {FullName}, {Organization}", dto.FullName, dto.Organization);
            return Result.Fail("حدث خطأ في قاعدة البيانات أثناء إضافة الضيف. يرجى المحاولة لاحقاً.");
        }
    }

    /// <summary>
    /// تحديث الضيف Async.
    /// </summary>
    public async Task<Result> UpdateGuestAsync(GuestDto dto, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.GuestManage);
        if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var guest = await context.Guests.FindAsync(dto.GuestId);

            if (guest == null) return Result.Fail("الضيف غير موجود.");

            guest.FullName = dto.FullName;
            guest.Organization = dto.Organization;
            guest.PhoneNumber = dto.PhoneNumber;
            guest.EmailAddress = dto.EmailAddress;

            try
            {
                await context.SaveChangesAsync(cancellationToken);

                // ✨ إبطال كاش الضيوف بعد التعديل
                await _cachedLookup.InvalidateByEntity("Guest");

                return Result.Success();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.Single();
                var dbValues = await entry.GetDatabaseValuesAsync();

                if (dbValues == null)
                    return Result.Fail("تم حذف هذا السجل من قبل مستخدم آخر.");

                var diff = new Dictionary<string, object?>();
                foreach (var property in dbValues.Properties)
                {
                    diff[property.Name] = dbValues[property.Name];
                }

                throw new ConcurrencyException(diff);
            }
        }
        catch (ConcurrencyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Guest: {GuestId}, {FullName}", dto.GuestId, dto.FullName);
            return Result.Fail("حدث خطأ في قاعدة البيانات أثناء تعديل بيانات الضيف. يرجى المحاولة لاحقاً.");
        }
    }

    /// <summary>
    /// Soft Delete الضيف Async.
    /// </summary>
    public async Task<Result> SoftDeleteGuestAsync(int guestId, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.GuestManage);
        if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var hasExecutedEpisodes = await context.EpisodeGuests
                .AnyAsync(eg => eg.GuestId == guestId &&
                                eg.IsActive &&
                                (eg.Episode.StatusId == EpisodeStatusValues.Executed ||
                                 eg.Episode.StatusId == EpisodeStatusValues.Published), cancellationToken);

            if (hasExecutedEpisodes)
                return Result.Fail("لا يمكن حذف ضيف مرتبط بحلقات تم تنفيذها أو نشرها بالفعل.");

            var guest = await context.Guests.FindAsync(guestId);
            if (guest == null) return Result.Fail("الضيف غير موجود.");

            guest.IsActive = false;

            await context.SaveChangesAsync(cancellationToken);

            // ✨ إبطال كاش الضيوف بعد الحذف
            await _cachedLookup.InvalidateByEntity("Guest");

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to soft delete Guest: {GuestId}", guestId);
            return Result.Fail("حدث خطأ في قاعدة البيانات أثناء حذف الضيف.");
        }
    }
}