using System.Security.Claims;

namespace DataAccess.Services
{
    /// <summary>
    /// واجهة خدمة التقييم المركزي للصلاحيات (الأدوار والاستثناءات).
    /// </summary>
    public interface IPermissionEvaluationService
    {
        /// <summary>
        /// التحقق من صلاحية مستخدم بشكل متزامن (مناسب لـ Views و Tag Helpers).
        /// </summary>
        bool HasPermission(ClaimsPrincipal? principal, string permissionName);

        /// <summary>
        /// الحصول على قائمة الصلاحيات الفعالة (Effective Permissions) لمستخدم ودوره.
        /// </summary>
        Task<List<string>> GetEffectivePermissionsAsync(int userId, int roleId);

        /// <summary>
        /// الحصول على الاستثناءات الفردية للمستخدم (الممنوحة والمحظورة).
        /// </summary>
        Task<(List<string> Grants, List<string> Denies)> GetUserOverridesAsync(int userId);

        /// <summary>
        /// إفراغ التخزين المؤقت لمستخدم معين.
        /// </summary>
        void InvalidateUserCache(int userId);

        /// <summary>
        /// إفراغ التخزين المؤقت لدور معين.
        /// </summary>
        void InvalidateRoleCache(int roleId);
    }
}
