// ============================================================
// PlatformService — PlatformService
// ============================================================
// المسؤولية: تعريف PlatformService.
// ============================================================
using DataAccess.Common;
using DataAccess.DTOs;
using Domain.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Services;

/// <summary>
/// صنف PlatformService.
/// </summary>
public class PlatformService : IPlatformService
{
    private readonly IDbContextFactory<BroadcastWorkflowDBContext> _contextFactory;
    private readonly ICachedLookupService _cachedLookup;
    private readonly ILogger<PlatformService> _logger;
    private readonly IValidator<SocialMediaPlatformDto> _platformValidator;

    public PlatformService(
        IDbContextFactory<BroadcastWorkflowDBContext> contextFactory,
        ICachedLookupService cachedLookup,
        ILogger<PlatformService> logger,
        IValidator<SocialMediaPlatformDto> platformValidator)
    {
        _contextFactory = contextFactory;
        _cachedLookup = cachedLookup;
        _logger = logger;
        _platformValidator = platformValidator;
    }
    /// <summary>
    /// استرجاع النشط Async.
    /// </summary>
    public async Task<List<SocialMediaPlatformDto>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.SocialMediaPlatforms
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new SocialMediaPlatformDto(p.SocialMediaPlatformId, p.Name, p.Icon, p.BaseUrl))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// إنشاء Async.
    /// </summary>
    public async Task<Result<int>> CreateAsync(SocialMediaPlatformDto dto, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.StaffManage);
        if (!permCheck.IsSuccess) return Result<int>.Fail(permCheck.ErrorMessage!);

        var validation = await _platformValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return Result<int>.Fail(string.Join(Environment.NewLine, validation.Errors.Select(e => e.ErrorMessage)));

        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var platform = new SocialMediaPlatform
            {
                Name = dto.Name,
                Icon = dto.Icon,
                BaseUrl = dto.BaseUrl
            };

            context.SocialMediaPlatforms.Add(platform);
            await context.SaveChangesAsync(cancellationToken);
            await _cachedLookup.InvalidateByEntity("SocialMediaPlatform");

            return Result<int>.Success(platform.SocialMediaPlatformId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Platform: {PlatformName}", dto.Name);
            return Result<int>.Fail("حدث خطأ في قاعدة البيانات أثناء إضافة المنصة. يرجى المحاولة لاحقاً.");
        }
    }

    /// <summary>
    /// تحديث Async.
    /// </summary>
    public async Task<Result> UpdateAsync(SocialMediaPlatformDto dto, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.StaffManage);
        if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

        var validation = await _platformValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return Result.Fail(string.Join(Environment.NewLine, validation.Errors.Select(e => e.ErrorMessage)));

        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var platform = await context.SocialMediaPlatforms.FindAsync(dto.SocialMediaPlatformId);
            if (platform == null)
                return Result.Fail("المنصة غير موجودة.");

            platform.Name = dto.Name;
            platform.Icon = dto.Icon;
            platform.BaseUrl = dto.BaseUrl;

            await context.SaveChangesAsync(cancellationToken);
            await _cachedLookup.InvalidateByEntity("SocialMediaPlatform");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Platform: {PlatformId}, {PlatformName}", dto.SocialMediaPlatformId, dto.Name);
            return Result.Fail("حدث خطأ في قاعدة البيانات أثناء تعديل المنصة. يرجى المحاولة لاحقاً.");
        }
    }

    /// <summary>
    /// حذف Async.
    /// </summary>
    public async Task<Result> DeleteAsync(int platformId, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.StaffManage);
        if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var platform = await context.SocialMediaPlatforms.FindAsync(platformId);
            if (platform == null)
                return Result.Fail("المنصة غير موجودة.");

            platform.IsActive = false;
            await context.SaveChangesAsync(cancellationToken);
            await _cachedLookup.InvalidateByEntity("SocialMediaPlatform");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to soft delete Platform: {PlatformId}", platformId);
            return Result.Fail("حدث خطأ في قاعدة البيانات أثناء حذف المنصة. يرجى المحاولة لاحقاً.");
        }
    }
}
