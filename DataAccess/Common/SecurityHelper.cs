// ============================================================
// SecurityHelper — الصلاحيات
// ============================================================
// المسؤولية: تعريف الصلاحيات.
// ============================================================
namespace DataAccess.Common;

/// <summary>
/// دوال مساعدة للتحقق من صلاحيات المستخدم داخل الـ Services
/// <summary>
/// صنف الصلاحيات.
/// </summary>
/// </summary>
public static class SecurityHelper
{
    /// <summary>
    /// يتحقق من امتلاك الصلاحية المطلوبة ويُرجع Result يحمل رسالة خطأ واضحة عند الرفض.
    /// استخدمه في بداية كل عملية تحتاج صلاحية.
    /// <summary>
    /// تأكيد الصلاحية.
    /// </summary>
    /// </summary>
    public static Result EnsurePermission(this UserSession session, string permissionName)
    {
        if (session.HasPermission(permissionName))
            return Result.Success();

        return Result.Fail($"عذراً، لا تملك صلاحية ({permissionName}) لإتمام هذه العملية.");
    }
}
