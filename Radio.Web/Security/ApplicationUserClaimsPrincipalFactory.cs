using Domain.Identity;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Radio.Web.Security
{
    /// <summary>
    /// يُنشئ ClaimsPrincipal من ApplicationUser مع Claims خاصة:
    ///   - FullName: الاسم الكامل
    ///   - DomainUserId: معرّف المستخدم في نظام الهوية
    ///   - DomainRoleId: معرّف الدور في نظام الهوية
    ///   - Permission (متعدد): لكل صلاحية نشطة للدور
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

            // Claims مخصصة
            identity.AddClaim(new Claim("FullName", user.FullName ?? user.UserName ?? "مستخدم"));
            identity.AddClaim(new Claim("DomainUserId", user.Id.ToString()));
            identity.AddClaim(new Claim("DomainRoleId", user.RoleId.ToString()));

            if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
                identity.AddClaim(new Claim("Phone", user.PhoneNumber));

            // هل المستخدم من فئة SuperAdmin؟ (يتجاوز جدول الصلاحيات)
            var roleNames = await UserManager.GetRolesAsync(user);
            var roleName = roleNames.FirstOrDefault();
            ApplicationRole? appRole = null;

            if (!string.IsNullOrEmpty(roleName))
            {
                appRole = await RoleManager.FindByNameAsync(roleName);
            }

            if (appRole?.IsSuperAdmin == true)
            {
                identity.AddClaim(new Claim("SuperAdmin", "true"));
            }

            // تحميل الصلاحيات من جدول AspNetRoleClaims عبر RoleId
            if (user.RoleId > 0)
            {
                try
                {
                    await using var context = await _contextFactory.CreateDbContextAsync();
                    var permissions = await context.RoleClaims
                        .AsNoTracking()
                        .Where(rc => rc.RoleId == user.RoleId && rc.ClaimType == "Permission" && rc.ClaimValue != null)
                        .Select(rc => rc.ClaimValue!)
                        .ToListAsync();

                    foreach (var permission in permissions)
                    {
                        identity.AddClaim(new Claim("Permission", permission));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "فشل تحميل الصلاحيات للدور {RoleId} للمستخدم {UserName}", user.RoleId, user.UserName);
                }
            }

            return identity;
        }
    }
}
