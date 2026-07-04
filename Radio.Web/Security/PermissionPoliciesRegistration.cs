// ============================================================
// PermissionPoliciesRegistration — الصلاحية Policies Registration
// ============================================================
// المسؤولية: تعريف الصلاحية Policies Registration.
// ============================================================
using DataAccess.Common;
using System.Reflection;

namespace Radio.Web.Security;

/// <summary>
/// PermissionPoliciesRegistration: صنف PermissionPoliciesRegistration.
/// <summary>
/// صنف PermissionPoliciesRegistration.
/// </summary>
/// <summary>
/// صنف الصلاحية Policies Registration.
/// </summary>
/// <summary>
/// صنف الصلاحية Policies Registration.
/// </summary>
/// <summary>
/// صنف الصلاحية Policies Registration.
/// </summary>
/// <summary>
/// صنف الصلاحية Policies Registration.
/// </summary>
/// <summary>
/// صنف الصلاحية Policies Registration.
/// </summary>
/// </summary>
public static class PermissionPoliciesRegistration
{
    /// <summary>
    /// معالجة Radio.Web.
    /// <summary>
    /// إضافة سياسات الصلاحيات.
    /// </summary>
    /// <summary>
    /// إضافة سياسات الصلاحيات.
    /// </summary>
    /// <summary>
    /// إضافة سياسات الصلاحيات.
    /// </summary>
    /// <summary>
    /// إضافة سياسات الصلاحيات.
    /// </summary>
    /// </summary>
    public static IServiceCollection AddPermissionPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            var permissionFields = typeof(AppPermissions)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f is { IsLiteral: true, IsInitOnly: false } && f.FieldType == typeof(string));

            foreach (var field in permissionFields)
            {
                var permissionName = (string)field.GetValue(null)!;
                options.AddPolicy(permissionName, p => p.RequireAssertion(ctx =>
                    ctx.User.HasPermission(permissionName)));
            }
        });

        return services;
    }
}
