// ============================================================
// CachedLookupService — البيانات المخبأة
// ============================================================
// المسؤولية: تعريف البيانات المخبأة.
// ============================================================
using DataAccess.DTOs;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess.Services;

/// <summary>
/// واجهة I Cached Lookup.
/// </summary>
public interface ICachedLookupService
{
    Task<List<StaffRoleDto>> GetStaffRolesAsync(CancellationToken cancellationToken = default);
    Task<List<ProgramDto>> GetProgramsAsync(CancellationToken cancellationToken = default);
    Task<List<GuestDto>> GetGuestsAsync(CancellationToken cancellationToken = default);
    Task<List<CorrespondentDto>> GetCorrespondentsAsync(CancellationToken cancellationToken = default);
    Task<List<EmployeeDto>> GetEmployeesAsync(CancellationToken cancellationToken = default);
    Task<List<SocialMediaPlatformDto>> GetSocialPlatformsAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<byte, string>> GetEpisodeStatusesAsync(CancellationToken cancellationToken = default);
    Task Invalidate(string key, CancellationToken cancellationToken = default);
    Task InvalidateAll(CancellationToken cancellationToken = default);
    Task InvalidateByEntity(string entityName, CancellationToken cancellationToken = default);
}

/// <summary>
/// صنف البيانات المخبأة.
/// </summary>
public class CachedLookupService : ICachedLookupService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    private readonly HybridCache _cache;
    private readonly IServiceScopeFactory _scopeFactory;

    public CachedLookupService(HybridCache cache, IServiceScopeFactory scopeFactory)
    {
        _cache = cache;
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// استرجاع الموظفين الأدوار Async.
    /// </summary>
    public async Task<List<StaffRoleDto>> GetStaffRolesAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync("lookup:staffroles", async _ =>
        {
            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BroadcastWorkflowDBContext>>();
            await using var context = await ctx.CreateDbContextAsync();
            return await context.StaffRoles
                .AsNoTracking()
                .Select(r => new StaffRoleDto(r.StaffRoleId, r.RoleName))
                .ToListAsync(cancellationToken);
        }, new HybridCacheEntryOptions { Expiration = CacheDuration }) ?? [];
    }

    /// <summary>
    /// استرجاع البرامج Async.
    /// </summary>
    public async Task<List<ProgramDto>> GetProgramsAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync("lookup:programs", async _ =>
        {
            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BroadcastWorkflowDBContext>>();
            await using var context = await ctx.CreateDbContextAsync();
            return await context.Programs
                .AsNoTracking()
                .Select(p => new ProgramDto(p.ProgramId, p.ProgramName, p.Category, p.ProgramDescription))
                .ToListAsync(cancellationToken);
        }, new HybridCacheEntryOptions { Expiration = CacheDuration }) ?? [];
    }

    /// <summary>
    /// استرجاع الضيوف Async.
    /// </summary>
    public async Task<List<GuestDto>> GetGuestsAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync("lookup:guests", async _ =>
        {
            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BroadcastWorkflowDBContext>>();
            await using var context = await ctx.CreateDbContextAsync();
            return await context.Guests
                .AsNoTracking()
                .Select(g => new GuestDto(g.GuestId, g.FullName, g.Organization, g.PhoneNumber, g.EmailAddress, string.Empty, string.Empty))
                .ToListAsync(cancellationToken);
        }, new HybridCacheEntryOptions { Expiration = CacheDuration }) ?? [];
    }

    /// <summary>
    /// استرجاع المراسلين Async.
    /// </summary>
    public async Task<List<CorrespondentDto>> GetCorrespondentsAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync("lookup:correspondents", async _ =>
        {
            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BroadcastWorkflowDBContext>>();
            await using var context = await ctx.CreateDbContextAsync();
            return await context.Correspondents
                .AsNoTracking()
                .Select(c => new CorrespondentDto(c.CorrespondentId, c.FullName, c.PhoneNumber, c.AssignedLocations))
                .ToListAsync(cancellationToken);
        }, new HybridCacheEntryOptions { Expiration = CacheDuration }) ?? [];
    }

    /// <summary>
    /// استرجاع Employees Async.
    /// </summary>
    public async Task<List<EmployeeDto>> GetEmployeesAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync("lookup:employees", async _ =>
        {
            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BroadcastWorkflowDBContext>>();
            await using var context = await ctx.CreateDbContextAsync();
            return await context.Employees
                .AsNoTracking()
                .Where(e => e.IsActive)
                .Select(e => new EmployeeDto(e.EmployeeId, e.FullName, e.StaffRoleId, e.StaffRole!.RoleName, e.Notes))
                .ToListAsync(cancellationToken);
        }, new HybridCacheEntryOptions { Expiration = CacheDuration }) ?? [];
    }

    /// <summary>
    /// استرجاع Social Platforms Async.
    /// </summary>
    public async Task<List<SocialMediaPlatformDto>> GetSocialPlatformsAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync("platforms", async _ =>
        {
            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BroadcastWorkflowDBContext>>();
            await using var context = await ctx.CreateDbContextAsync();
            return await context.SocialMediaPlatforms
                .AsNoTracking()
                .Select(p => new SocialMediaPlatformDto(p.SocialMediaPlatformId, p.Name, p.Icon, p.BaseUrl ?? string.Empty))
                .ToListAsync(cancellationToken);
        }, new HybridCacheEntryOptions { Expiration = CacheDuration }) ?? [];
    }

    /// <summary>
    /// استرجاع الحلقة Statuses Async.
    /// </summary>
    public async Task<Dictionary<byte, string>> GetEpisodeStatusesAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync("lookup:episodestatuses", async _ =>
        {
            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BroadcastWorkflowDBContext>>();
            await using var context = await ctx.CreateDbContextAsync();
            return await context.EpisodeStatuses
                .AsNoTracking()
                .ToDictionaryAsync(s => s.StatusId, s => s.DisplayName, cancellationToken);
        }, new HybridCacheEntryOptions { Expiration = CacheDuration }) ?? [];
    }

    /// <summary>
    /// Invalidate.
    /// </summary>
    public async Task Invalidate(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key);
    }

    /// <summary>
    /// Invalidate الكل.
    /// </summary>
    public async Task InvalidateAll(CancellationToken cancellationToken = default)
    {
        foreach (var key in s_allKeys)
            await _cache.RemoveAsync(key);
    }

    private static readonly string[] s_allKeys =
    [
        "lookup:staffroles", "lookup:programs", "lookup:guests",
        "lookup:correspondents", "lookup:employees", "lookup:episodestatuses", "platforms"
    ];

    private static readonly Dictionary<string, string[]> EntityCacheMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Guest"] = new[] { "lookup:guests" },
        ["Correspondent"] = new[] { "lookup:correspondents" },
        ["Program"] = new[] { "lookup:programs" },
        ["StaffRole"] = new[] { "lookup:staffroles" },
        ["SocialMediaPlatform"] = new[] { "platforms" },
        ["Employee"] = new[] { "lookup:employees" },
        ["Episode"] = new[] { "lookup:episodestatuses" },
    };

    /// <summary>
    /// Invalidate By Entity.
    /// </summary>
    public async Task InvalidateByEntity(string entityName, CancellationToken cancellationToken = default)
    {
        if (EntityCacheMap.TryGetValue(entityName, out var keys))
        {
            foreach (var key in keys)
                await _cache.RemoveAsync(key);
        }
    }
}
