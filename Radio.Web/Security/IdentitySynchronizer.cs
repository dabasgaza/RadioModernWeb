using Domain.Identity;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Radio.Web.Security;

/// <summary>
/// خدمة المزامنة بين ASP.NET Core Identity والنظام الأصلي.
///
/// عند إنشاء/تحديث مستخدم عبر Identity:
///   1. يُنشئ/يحدّث سجل في جدول Users الأصلي (للـ Audit Trail والـ FKs)
///   2. يُنشئ/يحدّث سجل ApplicationUser في Identity
///   3. يربطهما عبر ApplicationUser.DomainUserId
///
/// عند تغيير كلمة المرور:
///   - Identity يتكفّل بالـ Hash (عبر BCryptPasswordHasher)
///   - نُحدّث User.PasswordHash في Domain أيضاً (للتوافق مع النظام القديم)
/// </summary>
public interface IIdentitySynchronizer
{
    /// <summary>ينشئ مستخدم جديد في Identity + Domain User مرتبط.</summary>
    Task<(ApplicationUser IdentityUser, User DomainUser)> CreateUserAsync(
        string username, string password, string fullName,
        string emailAddress, string phoneNumber, int domainRoleId,
        bool isActive = true, CancellationToken cancellationToken = default);

    /// <summary>يحدّث بيانات المستخدم في Identity + Domain.</summary>
    Task UpdateUserAsync(
        ApplicationUser identityUser, string fullName,
        string emailAddress, string phoneNumber, int domainRoleId,
        bool isActive, CancellationToken cancellationToken = default);

    /// <summary>يحدّث كلمة المرور في Identity + Domain.</summary>
    Task UpdatePasswordAsync(ApplicationUser identityUser, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>يبني ApplicationUser جديد من Domain User موجود.</summary>
    Task<ApplicationUser?> BuildFromDomainUserAsync(int domainUserId, CancellationToken cancellationToken = default);

    /// <summary>يبني ApplicationRole جديد من Domain Role موجود.</summary>
    Task<ApplicationRole?> BuildFromDomainRoleAsync(int domainRoleId, CancellationToken cancellationToken = default);
}

public class IdentitySynchronizer : IIdentitySynchronizer
{
    private readonly ApplicationUserManager _userManager;
    private readonly ApplicationRoleManager _roleManager;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
    private readonly IDbContextFactory<BroadcastWorkflowDBContext> _contextFactory;
    private readonly ILogger<IdentitySynchronizer> _logger;

    public IdentitySynchronizer(
        ApplicationUserManager userManager,
        ApplicationRoleManager roleManager,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IDbContextFactory<BroadcastWorkflowDBContext> contextFactory,
        ILogger<IdentitySynchronizer> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _passwordHasher = passwordHasher;
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<(ApplicationUser IdentityUser, User DomainUser)> CreateUserAsync(
        string username, string password, string fullName,
        string emailAddress, string phoneNumber, int domainRoleId,
        bool isActive = true, CancellationToken cancellationToken = default)
    {
        // 1) إنشاء Domain User أولاً (للحصول على UserId)
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var now = DateTime.UtcNow;

        var domainUser = new User
        {
            Username = username,
            PasswordHash = string.Empty,
            FullName = fullName,
            EmailAddress = emailAddress ?? string.Empty,
            PhoneNumber = phoneNumber ?? string.Empty,
            RoleId = domainRoleId,
            IsActive = isActive,
            CreatedAt = now,
            UpdatedAt = now,
            RowVersion = Array.Empty<byte>()
        };
        context.Users.Add(domainUser);
        await context.SaveChangesAsync(cancellationToken);

        // 2) إنشاء ApplicationUser مرتبط
        var identityUser = new ApplicationUser
        {
            UserName = username,
            Email = emailAddress,
            FullName = fullName,
            DisplayPhoneNumber = phoneNumber,
            DomainUserId = domainUser.UserId,
            DomainRoleId = domainRoleId,
            IsActive = isActive,
            EmailConfirmed = true  // نتجاوز تأكيد البريد
        };

        var createResult = await _userManager.CreateAsync(identityUser, password);
        if (!createResult.Succeeded)
        {
            // تراجع: احذف Domain User
            context.Users.Remove(domainUser);
            await context.SaveChangesAsync(cancellationToken);

            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"فشل إنشاء Identity User: {errors}");
        }

        // نسخ الـ hash من Identity إلى Domain User (مصدر واحد للحقيقة)
        domainUser.PasswordHash = identityUser.PasswordHash ?? string.Empty;
        await context.SaveChangesAsync(cancellationToken);

        // 3) ربط الدور في Identity (نعكس DomainRoleId كـ Identity Role Name)
        var roleName = await context.Roles
            .Where(r => r.RoleId == domainRoleId)
            .Select(r => r.RoleName)
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrEmpty(roleName))
        {
            // تأكد من وجود ApplicationRole مطابق
            var appRole = await _roleManager.FindByNameAsync(roleName);
            if (appRole == null)
            {
                appRole = new ApplicationRole
                {
                    Name = roleName,
                    RoleDescription = "مزامنة تلقائية من Domain",
                    DomainRoleId = domainRoleId,
                    IsActive = true
                };
                await _roleManager.CreateAsync(appRole);
            }
            await _userManager.AddToRoleAsync(identityUser, roleName);
        }

        return (identityUser, domainUser);
    }

    public async Task UpdateUserAsync(
        ApplicationUser identityUser, string fullName,
        string emailAddress, string phoneNumber, int domainRoleId,
        bool isActive, CancellationToken cancellationToken = default)
    {
        // 1) تحديث Domain User
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var domainUser = await context.Users.FindAsync(identityUser.DomainUserId, cancellationToken);
        if (domainUser != null)
        {
            domainUser.FullName = fullName;
            domainUser.EmailAddress = emailAddress ?? string.Empty;
            domainUser.PhoneNumber = phoneNumber ?? string.Empty;
            domainUser.RoleId = domainRoleId;
            domainUser.IsActive = isActive;
            domainUser.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }

        // 2) تحديث ApplicationUser
        identityUser.FullName = fullName;
        identityUser.Email = emailAddress;
        identityUser.DisplayPhoneNumber = phoneNumber;
        identityUser.DomainRoleId = domainRoleId;
        identityUser.IsActive = isActive;
        await _userManager.UpdateAsync(identityUser);

        // 3) تحديث الأدوار في Identity
        var currentRoles = await _userManager.GetRolesAsync(identityUser);
        if (currentRoles.Any())
            await _userManager.RemoveFromRolesAsync(identityUser, currentRoles);

        var roleName = await context.Roles
            .Where(r => r.RoleId == domainRoleId)
            .Select(r => r.RoleName)
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrEmpty(roleName))
        {
            var appRole = await _roleManager.FindByNameAsync(roleName);
            if (appRole == null)
            {
                appRole = new ApplicationRole
                {
                    Name = roleName,
                    RoleDescription = "مزامنة تلقائية من Domain",
                    DomainRoleId = domainRoleId,
                    IsActive = true
                };
                await _roleManager.CreateAsync(appRole);
            }
            await _userManager.AddToRoleAsync(identityUser, roleName);
        }
    }

    public async Task UpdatePasswordAsync(ApplicationUser identityUser, string newPassword, CancellationToken cancellationToken = default)
    {
        // 1) تحديث Identity
        var token = await _userManager.GeneratePasswordResetTokenAsync(identityUser);
        var result = await _userManager.ResetPasswordAsync(identityUser, token, newPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"فشل تحديث كلمة المرور: {errors}");
        }

        // 2) تحديث Domain User بنفس الـ hash من Identity
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var domainUser = await context.Users.FindAsync(identityUser.DomainUserId, cancellationToken);
        if (domainUser != null)
        {
            domainUser.PasswordHash = identityUser.PasswordHash ?? string.Empty;
            domainUser.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<ApplicationUser?> BuildFromDomainUserAsync(int domainUserId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var domainUser = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == domainUserId && u.IsActive, cancellationToken);

        if (domainUser == null) return null;

        // ابحث عن ApplicationUser موجود، أو أنشئ جديداً (دون حفظه)
        var existing = await _userManager.Users
            .FirstOrDefaultAsync(u => u.DomainUserId == domainUserId, cancellationToken);

        if (existing != null) return existing;

        return new ApplicationUser
        {
            UserName = domainUser.Username,
            Email = domainUser.EmailAddress,
            FullName = domainUser.FullName,
            DisplayPhoneNumber = domainUser.PhoneNumber,
            DomainUserId = domainUser.UserId,
            DomainRoleId = domainUser.RoleId,
            IsActive = domainUser.IsActive,
            EmailConfirmed = true
        };
    }

    public async Task<ApplicationRole?> BuildFromDomainRoleAsync(int domainRoleId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var domainRole = await context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RoleId == domainRoleId, cancellationToken);

        if (domainRole == null) return null;

        var existing = await _roleManager.Roles
            .FirstOrDefaultAsync(r => r.DomainRoleId == domainRoleId, cancellationToken);

        if (existing != null) return existing;

        return new ApplicationRole
        {
            Name = domainRole.RoleName,
            RoleDescription = domainRole.RoleDescription,
            DomainRoleId = domainRole.RoleId,
            IsActive = domainRole.IsActive
        };
    }
}
