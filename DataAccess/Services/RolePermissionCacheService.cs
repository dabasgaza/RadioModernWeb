using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace DataAccess.Services
{
    /// <summary>
    /// واجهة خدمة التخزين المؤقت لصلاحيات الأدوار.
    /// </summary>
    public interface IRolePermissionCacheService
    {
        /// <summary>
        /// الحصول على الصلاحيات المرتبطة بدور معين من الذاكرة المؤقتة أو قاعدة البيانات.
        /// </summary>
        Task<List<string>> GetPermissionsForRoleAsync(int roleId);

        /// <summary>
        /// إفراغ التخزين المؤقت لدور معين.
        /// </summary>
        void Invalidate(int roleId);

        /// <summary>
        /// إفراغ جميع التخزين المؤقت.
        /// </summary>
        void InvalidateAll();
    }

    /// <summary>
    /// تطبيق خدمة التخزين المؤقت لصلاحيات الأدوار باستخدام IMemoryCache وقراءتها من AspNetRoleClaims.
    /// </summary>
    public class RolePermissionCacheService : IRolePermissionCacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDbContextFactory<BroadcastWorkflowDBContext> _contextFactory;
        private readonly ILogger<RolePermissionCacheService> _logger;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);
        private const string CacheKeyPrefix = "RolePermissions_";

        public RolePermissionCacheService(
            IMemoryCache memoryCache,
            IDbContextFactory<BroadcastWorkflowDBContext> contextFactory,
            ILogger<RolePermissionCacheService> logger)
        {
            _memoryCache = memoryCache;
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<List<string>> GetPermissionsForRoleAsync(int roleId)
        {
            if (roleId <= 0) return new List<string>();

            string cacheKey = $"{CacheKeyPrefix}{roleId}";

            if (!_memoryCache.TryGetValue(cacheKey, out List<string>? permissions) || permissions == null)
            {
                _logger.LogInformation("Cache miss for role permissions. Loading from database for RoleId: {RoleId}", roleId);
                permissions = await LoadPermissionsFromDatabaseAsync(roleId);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(CacheDuration)
                    .SetAbsoluteExpiration(TimeSpan.FromDays(2));

                _memoryCache.Set(cacheKey, permissions, cacheOptions);
            }

            return permissions;
        }

        public void Invalidate(int roleId)
        {
            string cacheKey = $"{CacheKeyPrefix}{roleId}";
            _memoryCache.Remove(cacheKey);
            _logger.LogInformation("Invalidated cache for RoleId: {RoleId}", roleId);
        }

        public void InvalidateAll()
        {
            _logger.LogInformation("InvalidateAll requested (roles will be invalidated individually on change).");
        }

        private async Task<List<string>> LoadPermissionsFromDatabaseAsync(int roleId)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                return await context.RoleClaims
                    .AsNoTracking()
                    .Where(rc => rc.RoleId == roleId && rc.ClaimType == "Permission" && rc.ClaimValue != null)
                    .Select(rc => rc.ClaimValue!)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading permissions from database for RoleId: {RoleId}", roleId);
                return new List<string>();
            }
        }
    }
}
