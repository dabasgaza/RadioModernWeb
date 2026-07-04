using DataAccess.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using System;
using System.Linq;
using System.Security.Claims;

namespace Radio.Web.Security
{
    /// <summary>
    /// أحداث مخصصة لمصادقة الكوكيز لإعادة بناء وتحديث صلاحيات المستخدم ديناميكياً عند كل طلب.
    /// </summary>
    public class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
    {
        private readonly IRolePermissionCacheService _permissionCache;
        private readonly ILogger<CustomCookieAuthenticationEvents> _logger;

        public CustomCookieAuthenticationEvents(
            IRolePermissionCacheService permissionCache,
            ILogger<CustomCookieAuthenticationEvents> logger)
        {
            _permissionCache = permissionCache;
            _logger = logger;
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var principal = context.Principal;
            if (principal?.Identity?.IsAuthenticated == true)
            {
                var roleIdClaim = principal.FindFirst("DomainRoleId");
                if (roleIdClaim != null && int.TryParse(roleIdClaim.Value, out var roleId))
                {
                    // جلب الصلاحيات المحدثة فورياً من التخزين المؤقت
                    var permissions = await _permissionCache.GetPermissionsForRoleAsync(roleId);

                    if (principal.Identity is ClaimsIdentity identity)
                    {
                        // إزالة الصلاحيات القديمة من الهوية الحالية لمنع تراكم التكرار
                        var existingPermissionClaims = identity.FindAll("Permission").ToList();
                        foreach (var claim in existingPermissionClaims)
                        {
                            identity.RemoveClaim(claim);
                        }

                        // إضافة الصلاحيات النشطة الجديدة
                        foreach (var permission in permissions)
                        {
                            identity.AddClaim(new Claim("Permission", permission));
                        }
                    }
                }
            }

            await base.ValidatePrincipal(context);
        }
    }
}
