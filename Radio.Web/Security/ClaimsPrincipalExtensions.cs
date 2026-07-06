// ============================================================
// ClaimsPrincipalExtensions — Claims Principal Extensions
// ============================================================
// المسؤولية: تعريف Claims Principal Extensions والتحقق الفعال.
// ============================================================
using DataAccess.Services;
using System.Security.Claims;

namespace Radio.Web.Security;

/// <summary>
/// Extension methods for ClaimsPrincipal to check permissions easily in Razor Views.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>هل المستخدم يملك صلاحية معيّنة؟ (SuperAdmin يملك كل الصلاحيات)</summary>
    public static bool HasPermission(this ClaimsPrincipal user, string permissionName)
    {
        if (user?.Identity?.IsAuthenticated != true) return false;

        // SuperAdmin bypasses role-permission table
        if (user.HasClaim(c => c.Type == "SuperAdmin")) return true;

        // محاولة استخدام محرك تقييم الصلاحيات الجديد والمرن
        var httpContext = HttpContextHolder.Current;
        if (httpContext != null)
        {
            var evaluationService = httpContext.RequestServices.GetService<IPermissionEvaluationService>();
            if (evaluationService != null)
            {
                return evaluationService.HasPermission(user, permissionName);
            }
        }

        // Fallback إلى الكليّمات المباشرة في الكوكي في حال عدم توفر سياق الطلب
        return user.HasClaim("Permission", permissionName);
    }

    /// <summary>الحصول على معرّف المستخدم في Domain (int)</summary>
    public static int GetDomainUserId(this ClaimsPrincipal user)
        => int.TryParse(user?.FindFirstValue("DomainUserId"), out var id) ? id : 0;

    /// <summary>الحصول على الاسم الكامل</summary>
    public static string? GetFullName(this ClaimsPrincipal user)
        => user?.FindFirstValue("FullName");
}
