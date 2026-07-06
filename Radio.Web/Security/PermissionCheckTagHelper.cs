using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Radio.Web.Security
{
    /// <summary>
    /// Tag Helper مخصص لتسهيل التحقق من الصلاحيات في ملفات Razor Views.
    /// الاستخدام: &lt;permission-check permission="Episodes.Edit"&gt; ... &lt;/permission-check&gt;
    /// </summary>
    [HtmlTargetElement("permission-check")]
    public class PermissionCheckTagHelper : TagHelper
    {
        /// <summary>
        /// اسم الصلاحية المطلوب فحصها.
        /// </summary>
        [HtmlAttributeName("permission")]
        public string Permission { get; set; } = string.Empty;

        /// <summary>
        /// سياق الصفحة الحالية.
        /// </summary>
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; } = null!;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // إخفاء الحاوية الخارجية وعرض المحتوى الداخلي فقط
            output.TagName = null;

            var user = ViewContext.HttpContext?.User;
            if (user == null || !user.HasPermission(Permission))
            {
                output.SuppressOutput();
            }
        }
    }
}
