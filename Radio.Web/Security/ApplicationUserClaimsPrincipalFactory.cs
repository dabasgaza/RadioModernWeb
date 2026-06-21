using System.Security.Claims;
using Domain.Identity;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Radio.Web.Security;

/// <summary>
/// يُنشئ ClaimsPrincipal من ApplicationUser مع Claims خاصة:
///   - FullName: الاسم الكامل
///   - DomainUserId: معرّف المستخدم في النظام الأصلي
///   - DomainRoleId: معرّف الدور في النظام الأصلي
///   - Permission (متعدد): لكل صلاحية في النظام الأصلي
///
/// تتم قراءة الصلاحيات من جدول RolePermissions الأصلي لضمان التوافق التام.
/// </summary>
public class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
{
    private readonly IDbContextFactory<BroadcastWorkflowDBContext> _contextFactory;
    private readonly ILogger<ApplicationUserClaimsPrincipalFactory> _logger;

    public ApplicationUserClaimsPrincipalFactory(
        ApplicationUserManager userManager,
        ApplicationRoleManager roleManager,
        IOptions<IdentityOptions> optionsAccessor,
        IDbContextFactory<BroadcastWorkflowDBContext> contextFactory,
        ILogger<ApplicationUserClaimsPrincipalFactory> logger)
        : base(userManager, roleManager, optionsAccessor)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        // إصلاح تلقائي لـ DomainUserId = 0 (مستخدمين قدامى لم تتم مزامنتهم)
        if (user.DomainUserId == 0)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var domainUser = await context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username == user.UserName && u.IsActive);

                if (domainUser != null)
                {
                    user.DomainUserId = domainUser.UserId;
                    user.DomainRoleId = domainUser.RoleId;
                    await UserManager.UpdateAsync(user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "فشل الإصلاح التلقائي لـ DomainUserId للمستخدم {UserName}", user.UserName);
            }
        }

        // Claims مخصصة
        identity.AddClaim(new Claim("FullName", user.FullName ?? user.UserName ?? "مستخدم"));
        identity.AddClaim(new Claim("DomainUserId", user.DomainUserId.ToString()));
        identity.AddClaim(new Claim("DomainRoleId", user.DomainRoleId.ToString()));
        if (!string.IsNullOrWhiteSpace(user.DisplayPhoneNumber))
            identity.AddClaim(new Claim("Phone", user.DisplayPhoneNumber));

        // هل المستخدم من فئة SuperAdmin؟ (يتجاوز جدول الصلاحيات)
        var appRole = await RoleManager.Roles.FirstOrDefaultAsync(r => r.DomainRoleId == user.DomainRoleId);
        if (appRole?.IsSuperAdmin == true)
            identity.AddClaim(new Claim("SuperAdmin", "true"));

        // تحميل الصلاحيات من جدول RolePermissions الأصلي عبر DomainRoleId
        if (user.DomainRoleId > 0)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var permissions = await context.RolePermissions
                    .AsNoTracking()
                    .Where(rp => rp.RoleId == user.DomainRoleId)
                    .Select(rp => rp.Permission.SystemName)
                    .ToListAsync();

                foreach (var permission in permissions)
                {
                    identity.AddClaim(new Claim("Permission", permission));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "فشل تحميل الصلاحيات للدور {DomainRoleId} للمستخدم {UserName}", user.DomainRoleId, user.UserName);
            }
        }

        return identity;
    }
}
