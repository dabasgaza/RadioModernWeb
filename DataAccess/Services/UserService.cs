using DataAccess.Common;
using DataAccess.DTOs;
using Domain.Identity;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace DataAccess.Services
{
    /// <summary>
    /// واجهة خدمة إدارة المستخدمين والأدوار.
    /// </summary>
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
        Task<Result> CreateUserAsync(UserDto dto, string plainPassword, UserSession session, CancellationToken cancellationToken = default);
        Task<Result> UpdateUserAsync(UserDto dto, string? newPassword, UserSession session, CancellationToken cancellationToken = default);
        Task<Result> ToggleUserStatusAsync(int userId, bool isActive, UserSession session, CancellationToken cancellationToken = default);
        Task<List<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default);
        Task<Result> CreateRoleAsync(RoleDto dto, UserSession session, CancellationToken cancellationToken = default);
        Task<Result> UpdateRoleAsync(RoleDto dto, UserSession session, CancellationToken cancellationToken = default);
        Task<Result> DeleteRoleAsync(int roleId, UserSession session, CancellationToken cancellationToken = default);
        Task<List<PermissionViewModel>> GetPermissionsMatrixAsync(int roleId, CancellationToken cancellationToken = default);
        Task<Result> UpdateRolePermissionsAsync(int roleId, List<int> selectedPermissionIds, UserSession session, CancellationToken cancellationToken = default);
        Task<Result<int>> CloneRoleAsync(int sourceRoleId, string newRoleName, string newDescription, UserSession session, CancellationToken cancellationToken = default);
        Task<Result> DeleteUserAsync(int userId, UserSession session, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// تطبيق خدمة المستخدمين باستخدام ASP.NET Core Identity مباشرة.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IDbContextFactory<BroadcastWorkflowDBContext> _contextFactory;
        private readonly CurrentSessionProvider _sessionProvider;
        private readonly ILogger<UserService> _logger;
        private readonly IRolePermissionCacheService _permissionCache;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IPermissionService _permissionService;

        public UserService(
            IDbContextFactory<BroadcastWorkflowDBContext> contextFactory,
            CurrentSessionProvider sessionProvider,
            ILogger<UserService> logger,
            IRolePermissionCacheService permissionCache,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IPermissionService permissionService)
        {
            _contextFactory = contextFactory;
            _sessionProvider = sessionProvider;
            _logger = logger;
            _permissionCache = permissionCache;
            _userManager = userManager;
            _roleManager = roleManager;
            _permissionService = permissionService;
        }

        #region إدارة المستخدمين

        public async Task<List<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            return await context.Users
                .AsNoTracking()
                .OrderBy(u => u.UserName)
                .Select(u => new UserDto
                {
                    UserId = u.Id,
                    Username = u.UserName!,
                    FullName = u.FullName,
                    EmailAddress = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    IsActive = u.IsActive,
                    RoleName = context.UserRoles
                        .Where(ur => ur.UserId == u.Id)
                        .Join(context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                        .FirstOrDefault() ?? "Unknown"
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<Result> CreateUserAsync(UserDto dto, string plainPassword, UserSession session, CancellationToken cancellationToken = default)
        {
            var permCheck = session.EnsurePermission(AppPermissions.UserManage);
            if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

            if (await _userManager.FindByNameAsync(dto.Username) != null)
                return Result.Fail("اسم المستخدم موجود مسبقاً");

            var user = new ApplicationUser
            {
                UserName = dto.Username,
                Email = dto.EmailAddress,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, plainPassword);
            if (!result.Succeeded)
            {
                return Result.Fail(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            if (!string.IsNullOrEmpty(dto.RoleName))
            {
                var role = await _roleManager.FindByNameAsync(dto.RoleName);
                if (role != null)
                {
                    user.RoleId = role.Id;
                    await _userManager.UpdateAsync(user);
                    await _userManager.AddToRoleAsync(user, dto.RoleName);
                }
            }

            return Result.Success();
        }

        public async Task<Result> UpdateUserAsync(UserDto dto, string? newPassword, UserSession session, CancellationToken cancellationToken = default)
        {
            var permCheck = session.EnsurePermission(AppPermissions.UserManage);
            if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

            var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
            if (user == null) return Result.Fail("المستخدم غير موجود");

            var existingWithUsername = await _userManager.FindByNameAsync(dto.Username);
            if (existingWithUsername != null && existingWithUsername.Id != dto.UserId)
                return Result.Fail("اسم المستخدم موجود مسبقاً");

            user.UserName = dto.Username;
            user.FullName = dto.FullName;
            user.Email = dto.EmailAddress;
            user.PhoneNumber = dto.PhoneNumber;
            user.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(newPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
                if (!resetResult.Succeeded)
                {
                    return Result.Fail(string.Join(", ", resetResult.Errors.Select(e => e.Description)));
                }
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(dto.RoleName))
            {
                if (currentRoles.Count > 0)
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                }

                var role = await _roleManager.FindByNameAsync(dto.RoleName);
                if (role != null)
                {
                    user.RoleId = role.Id;
                    await _userManager.AddToRoleAsync(user, dto.RoleName);
                }
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return Result.Fail(string.Join(", ", updateResult.Errors.Select(e => e.Description)));
            }

            return Result.Success();
        }

        public async Task<Result> ToggleUserStatusAsync(int userId, bool isActive, UserSession session, CancellationToken cancellationToken = default)
        {
            var permCheck = session.EnsurePermission(AppPermissions.UserManage);
            if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return Result.Fail("المستخدم غير موجود");

            user.IsActive = isActive;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return Result.Fail(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return Result.Success();
        }

        public Task<Result> DeleteUserAsync(int userId, UserSession session, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Fail("حذف المستخدمين معطل حالياً، يرجى تعطيل حساب المستخدم بدلاً من الحذف."));
        }

        #endregion

        #region إدارة الأدوار والصلاحيات

        public async Task<List<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default)
        {
            return await _roleManager.Roles
                .AsNoTracking()
                .OrderBy(r => r.Name)
                .Select(r => new RoleDto
                {
                    RoleId = r.Id,
                    RoleName = r.Name!,
                    RoleDescription = r.RoleDescription
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<Result> CreateRoleAsync(RoleDto dto, UserSession session, CancellationToken cancellationToken = default)
        {
            var permCheck = session.EnsurePermission(AppPermissions.UserManage);
            if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

            if (await _roleManager.RoleExistsAsync(dto.RoleName))
                return Result.Fail("اسم الدور موجود مسبقاً");

            var role = new ApplicationRole
            {
                Name = dto.RoleName,
                RoleDescription = dto.RoleDescription,
                IsActive = true
            };

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                return Result.Fail(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return Result.Success();
        }

        public async Task<Result> UpdateRoleAsync(RoleDto dto, UserSession session, CancellationToken cancellationToken = default)
        {
            var permCheck = session.EnsurePermission(AppPermissions.UserManage);
            if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

            var role = await _roleManager.FindByIdAsync(dto.RoleId.ToString());
            if (role == null) return Result.Fail("الدور غير موجود");

            var existingWithRoleName = await _roleManager.FindByNameAsync(dto.RoleName);
            if (existingWithRoleName != null && existingWithRoleName.Id != dto.RoleId)
                return Result.Fail("اسم الدور موجود مسبقاً");

            role.Name = dto.RoleName;
            role.RoleDescription = dto.RoleDescription;

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                return Result.Fail(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return Result.Success();
        }

        public async Task<Result> DeleteRoleAsync(int roleId, UserSession session, CancellationToken cancellationToken = default)
        {
            var permCheck = session.EnsurePermission(AppPermissions.UserManage);
            if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null) return Result.Fail("الدور غير موجود");

            var hasUsers = await _userManager.Users.AnyAsync(u => u.RoleId == roleId, cancellationToken);
            if (hasUsers) return Result.Fail("لا يمكن حذف الدور لأنه مرتبط بمستخدمين حاليين");

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                return Result.Fail(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            _permissionCache.Invalidate(roleId);

            return Result.Success();
        }

        public async Task<List<PermissionViewModel>> GetPermissionsMatrixAsync(int roleId, CancellationToken cancellationToken = default)
        {
            var allPermissions = await _permissionService.GetPermissionsListAsync();
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var assignedClaims = await context.RoleClaims
                .AsNoTracking()
                .Where(rc => rc.RoleId == roleId && rc.ClaimType == "Permission")
                .Select(rc => rc.ClaimValue)
                .ToListAsync(cancellationToken);

            return allPermissions.Select(p => new PermissionViewModel
            {
                PermissionId = p.PermissionId,
                SystemName = p.SystemName,
                DisplayName = p.DisplayName,
                Module = p.Module,
                IsAssigned = assignedClaims.Contains(p.SystemName)
            }).ToList();
        }

        public async Task<Result> UpdateRolePermissionsAsync(int roleId, List<int> selectedPermissionIds, UserSession session, CancellationToken cancellationToken = default)
        {
            var permCheck = session.EnsurePermission(AppPermissions.UserManage);
            if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null) return Result.Fail("الدور غير موجود");

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var existingClaims = await context.RoleClaims
                .Where(rc => rc.RoleId == roleId && rc.ClaimType == "Permission")
                .ToListAsync(cancellationToken);

            if (existingClaims.Count > 0)
            {
                context.RoleClaims.RemoveRange(existingClaims);
                await context.SaveChangesAsync(cancellationToken);
            }

            var allPermissions = await _permissionService.GetPermissionsListAsync();
            var selectedPermissionNames = allPermissions
                .Where(p => selectedPermissionIds.Contains(p.PermissionId))
                .Select(p => p.SystemName)
                .ToList();

            if (selectedPermissionNames.Count > 0)
            {
                context.RoleClaims.AddRange(
                    selectedPermissionNames.Select(name => new IdentityRoleClaim<int>
                    {
                        RoleId = roleId,
                        ClaimType = "Permission",
                        ClaimValue = name
                    }));

                await context.SaveChangesAsync(cancellationToken);
            }

            await _sessionProvider.RefreshPermissionsAsync();
            _permissionCache.Invalidate(roleId);

            return Result.Success();
        }

        public async Task<Result<int>> CloneRoleAsync(int sourceRoleId, string newRoleName, string newDescription, UserSession session, CancellationToken cancellationToken = default)
        {
            var permCheck = session.EnsurePermission(AppPermissions.UserManage);
            if (!permCheck.IsSuccess) return Result<int>.Fail(permCheck.ErrorMessage!);

            if (await _roleManager.RoleExistsAsync(newRoleName))
                return Result<int>.Fail("اسم الدور الجديد موجود بالفعل");

            var sourceRole = await _roleManager.FindByIdAsync(sourceRoleId.ToString());
            if (sourceRole == null) return Result<int>.Fail("الدور المصدر غير موجود");

            var newRole = new ApplicationRole
            {
                Name = newRoleName,
                RoleDescription = newDescription,
                IsActive = true
            };

            var result = await _roleManager.CreateAsync(newRole);
            if (!result.Succeeded)
            {
                return Result<int>.Fail(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var sourceClaims = await context.RoleClaims
                .AsNoTracking()
                .Where(rc => rc.RoleId == sourceRoleId && rc.ClaimType == "Permission")
                .Select(rc => rc.ClaimValue)
                .ToListAsync(cancellationToken);

            if (sourceClaims.Count > 0)
            {
                context.RoleClaims.AddRange(
                    sourceClaims.Select(val => new IdentityRoleClaim<int>
                    {
                        RoleId = newRole.Id,
                        ClaimType = "Permission",
                        ClaimValue = val
                    }));

                await context.SaveChangesAsync(cancellationToken);
            }

            return Result<int>.Success(newRole.Id);
        }

        #endregion
    }
}