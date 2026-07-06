using Microsoft.AspNetCore.Authorization;

namespace Radio.Web.Security
{
    /// <summary>
    /// وسم (Attribute) مخصص لحماية الـ Controllers والـ Actions بالصلاحية المطلوبة.
    /// يختصر كتابة [Authorize(Policy = "...")] إلى [HasPermission("...")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(string permissionName) : base(permissionName)
        {
        }
    }
}
