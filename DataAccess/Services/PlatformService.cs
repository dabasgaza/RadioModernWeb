using DataAccess.Common;
using DataAccess.DTOs;
using DataAccess.Validation;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Services;

public class PlatformService : IPlatformService
{
    private readonly IDbContextFactory<BroadcastWorkflowDBContext> _contextFactory;
    private readonly ICachedLookupService _cachedLookup;
    private readonly ILogger<PlatformService> _logger;

    public PlatformService(
        IDbContextFactory<BroadcastWorkflowDBContext> contextFactory,
        ICachedLookupService cachedLookup,
        ILogger<PlatformService> logger)
    {
        _contextFactory = contextFactory;
        _cachedLookup = cachedLookup;
        _logger = logger;
    }
    public async Task<List<SocialMediaPlatformDto>> GetAllActiveAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.SocialMediaPlatforms
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new SocialMediaPlatformDto(p.SocialMediaPlatformId, p.Name, p.Icon, p.BaseUrl))
            .ToListAsync();
    }

    public async Task<Result<int>> CreateAsync(SocialMediaPlatformDto dto, UserSession session)
    {
        var permCheck = session.EnsurePermission(AppPermissions.StaffManage);
        if (!permCheck.IsSuccess) return Result<int>.Fail(permCheck.ErrorMessage!);

        var validation = ValidationPipeline.ValidatePlatform(dto);
        if (!validation.IsSuccess) return Result<int>.Fail(validation.ErrorMessage!);

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
            await context.SaveChangesAsync();
            _cachedLookup.InvalidateByEntity("SocialMediaPlatform");

            return Result<int>.Success(platform.SocialMediaPlatformId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Platform: {PlatformName}", dto.Name);
            return Result<int>.Fail("حدث خطأ في قاعدة البيانات أثناء إضافة المنصة. يرجى المحاولة لاحقاً.");
        }
    }

    public async Task<Result> UpdateAsync(SocialMediaPlatformDto dto, UserSession session)
    {
        var permCheck = session.EnsurePermission(AppPermissions.StaffManage);
        if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

        var validation = ValidationPipeline.ValidatePlatform(dto);
        if (!validation.IsSuccess) return Result.Fail(validation.ErrorMessage!);

        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var platform = await context.SocialMediaPlatforms.FindAsync(dto.SocialMediaPlatformId);
            if (platform == null)
                return Result.Fail("المنصة غير موجودة.");

            platform.Name = dto.Name;
            platform.Icon = dto.Icon;
            platform.BaseUrl = dto.BaseUrl;

            await context.SaveChangesAsync();
            _cachedLookup.InvalidateByEntity("SocialMediaPlatform");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Platform: {PlatformId}, {PlatformName}", dto.SocialMediaPlatformId, dto.Name);
            return Result.Fail("حدث خطأ في قاعدة البيانات أثناء تعديل المنصة. يرجى المحاولة لاحقاً.");
        }
    }

    public async Task<Result> DeleteAsync(int platformId, UserSession session)
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
            await context.SaveChangesAsync();
            _cachedLookup.InvalidateByEntity("SocialMediaPlatform");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to soft delete Platform: {PlatformId}", platformId);
            return Result.Fail("حدث خطأ في قاعدة البيانات أثناء حذف المنصة. يرجى المحاولة لاحقاً.");
        }
    }
}
