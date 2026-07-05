using Domain.Identity;
using Domain.Models;
using DataAccess.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace DataAccess.Services
{
    /// <summary>
    /// تطبيق الخدمة المركزية للتحقق من الصلاحيات الهجينة (الأدوار + الاستثناءات).
    /// </summary>
    public class PermissionEvaluationService : IPermissionEvaluationService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDbContextFactory<BroadcastWorkflowDBContext> _contextFactory;
        private readonly IRolePermissionCacheService _rolePermissionCache;
        private readonly ILogger<PermissionEvaluationService> _logger;

        private const string UserOverridesCacheKeyPrefix = "UserOverrides_";
        private const string RoleSuperAdminCacheKeyPrefix = "RoleSuperAdmin_";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

        public PermissionEvaluationService(
            IMemoryCache memoryCache,
            IDbContextFactory<BroadcastWorkflowDBContext> contextFactory,
            IRolePermissionCacheService rolePermissionCache,
            ILogger<PermissionEvaluationService> logger)
        {
            _memoryCache = memoryCache;
            _contextFactory = contextFactory;
            _rolePermissionCache = rolePermissionCache;
            _logger = logger;
        }

        public bool HasPermission(ClaimsPrincipal? principal, string permissionName)
        {
            if (principal?.Identity?.IsAuthenticated != true) return false;

            // تجاوز كامل للسوبر أدمن المباشر من الكليّمات
            if (principal.HasClaim(c => c.Type == "SuperAdmin") == true) return true;

            // استخراج معرف المستخدم والدور من الكليّمات
            var userIdClaim = principal.FindFirst("DomainUserId")?.Value;
            var roleIdClaim = principal.FindFirst("DomainRoleId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId)) return false;
            if (string.IsNullOrEmpty(roleIdClaim) || !int.TryParse(roleIdClaim, out var roleId)) return false;

            // التحقق من حالة السوبر أدمن للدور من الكاش
            if (IsRoleSuperAdmin(roleId)) return true;

            // جلب استثناءات المستخدم من الكاش
            var (grants, denies) = GetUserOverridesFromCache(userId);

            // 1. الحظر الفردي (Deny) له الأولوية القصوى
            if (denies.Contains(permissionName, StringComparer.OrdinalIgnoreCase)) return false;

            // 2. السماح الفردي المباشر (Grant)
            if (grants.Contains(permissionName, StringComparer.OrdinalIgnoreCase)) return true;

            // 3. صلاحيات الدور (Role Permissions)
            var rolePermissions = GetRolePermissionsFromCache(roleId);
            if (rolePermissions.Contains(permissionName, StringComparer.OrdinalIgnoreCase)) return true;

            return false;
        }

        public async Task<List<string>> GetEffectivePermissionsAsync(int userId, int roleId)
        {
            if (userId <= 0) return new List<string>();

            // السوبر أدمن يحصل على كافة الصلاحيات
            if (roleId > 0 && await IsRoleSuperAdminAsync(roleId))
            {
                var fields = typeof(AppPermissions).GetFields(
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Static |
                    System.Reflection.BindingFlags.FlattenHierarchy);

                return fields
                    .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                    .Select(f => (string)f.GetValue(null)!)
                    .ToList();
            }

            var rolePerms = roleId > 0 
                ? await _rolePermissionCache.GetPermissionsForRoleAsync(roleId) 
                : new List<string>();

            var (grants, denies) = await GetUserOverridesAsync(userId);

            // دمج: (صلاحيات الدور + السماح الفردي) - الحظر الفردي
            return rolePerms
                .Union(grants, StringComparer.OrdinalIgnoreCase)
                .Except(denies, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public async Task<(List<string> Grants, List<string> Denies)> GetUserOverridesAsync(int userId)
        {
            if (userId <= 0) return (new List<string>(), new List<string>());

            string cacheKey = $"{UserOverridesCacheKeyPrefix}{userId}";

            if (!_memoryCache.TryGetValue(cacheKey, out (List<string> Grants, List<string> Denies) overrides))
            {
                _logger.LogInformation("Cache miss for user overrides. Loading from database for UserId: {UserId}", userId);
                overrides = await LoadUserOverridesFromDatabaseAsync(userId);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(CacheDuration)
                    .SetAbsoluteExpiration(TimeSpan.FromDays(2));

                _memoryCache.Set(cacheKey, overrides, cacheOptions);
            }

            return overrides;
        }

        public void InvalidateUserCache(int userId)
        {
            string cacheKey = $"{UserOverridesCacheKeyPrefix}{userId}";
            _memoryCache.Remove(cacheKey);
            _logger.LogInformation("Invalidated user overrides cache for UserId: {UserId}", userId);
        }

        public void InvalidateRoleCache(int roleId)
        {
            _rolePermissionCache.Invalidate(roleId);
            string superAdminKey = $"{RoleSuperAdminCacheKeyPrefix}{roleId}";
            _memoryCache.Remove(superAdminKey);
            _logger.LogInformation("Invalidated role cache for RoleId: {RoleId}", roleId);
        }

        #region Private Helper Methods

        private (List<string> Grants, List<string> Denies) GetUserOverridesFromCache(int userId)
        {
            string cacheKey = $"{UserOverridesCacheKeyPrefix}{userId}";
            if (_memoryCache.TryGetValue(cacheKey, out (List<string> Grants, List<string> Denies) overrides))
            {
                return overrides;
            }

            // Fallback متزامن في حال لم تكن محملة مسبقاً (تجنباً للموت السريري في الواجهات)
            _logger.LogWarning("Synchronous user overrides cache miss for UserId: {UserId}. Fetching synchronously.", userId);
            var dbOverrides = Task.Run(() => LoadUserOverridesFromDatabaseAsync(userId)).GetAwaiter().GetResult();
            
            _memoryCache.Set(cacheKey, dbOverrides, new MemoryCacheEntryOptions().SetSlidingExpiration(CacheDuration));
            return dbOverrides;
        }

        private List<string> GetRolePermissionsFromCache(int roleId)
        {
            // استدعاء الميثود المتزامنة أو تشغيلها عبر Task.Run لتجنب تعليق الطلب
            return Task.Run(() => _rolePermissionCache.GetPermissionsForRoleAsync(roleId)).GetAwaiter().GetResult();
        }

        private bool IsRoleSuperAdmin(int roleId)
        {
            if (roleId <= 0) return false;
            string cacheKey = $"{RoleSuperAdminCacheKeyPrefix}{roleId}";

            if (_memoryCache.TryGetValue(cacheKey, out bool isSuperAdmin))
            {
                return isSuperAdmin;
            }

            bool dbIsSuperAdmin = Task.Run(() => IsRoleSuperAdminAsync(roleId)).GetAwaiter().GetResult();
            _memoryCache.Set(cacheKey, dbIsSuperAdmin, new MemoryCacheEntryOptions().SetSlidingExpiration(CacheDuration));
            return dbIsSuperAdmin;
        }

        private async Task<bool> IsRoleSuperAdminAsync(int roleId)
        {
            if (roleId <= 0) return false;
            string cacheKey = $"{RoleSuperAdminCacheKeyPrefix}{roleId}";

            if (!_memoryCache.TryGetValue(cacheKey, out bool isSuperAdmin))
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var role = await context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == roleId);
                isSuperAdmin = role?.IsSuperAdmin == true;
                _memoryCache.Set(cacheKey, isSuperAdmin, new MemoryCacheEntryOptions().SetSlidingExpiration(CacheDuration));
            }

            return isSuperAdmin;
        }

        private async Task<(List<string> Grants, List<string> Denies)> LoadUserOverridesFromDatabaseAsync(int userId)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                
                var claims = await context.UserClaims
                    .AsNoTracking()
                    .Where(uc => uc.UserId == userId && (uc.ClaimType == "Permission" || uc.ClaimType == "PermissionDeny") && uc.ClaimValue != null)
                    .ToListAsync();

                var grants = claims.Where(c => c.ClaimType == "Permission").Select(c => AppPermissions.Normalize(c.ClaimValue!)).ToList();
                var denies = claims.Where(c => c.ClaimType == "PermissionDeny").Select(c => AppPermissions.Normalize(c.ClaimValue!)).ToList();

                return (grants, denies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user overrides from database for UserId: {UserId}", userId);
                return (new List<string>(), new List<string>());
            }
        }

        #endregion
    }
}
