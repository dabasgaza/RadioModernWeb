namespace DataAccess.DTOs
{
    /// <summary>
    /// نموذج عرض حالة صلاحيات المستخدم والاستثناءات المطبقة عليها.
    /// </summary>
    public class UserPermissionOverrideViewModel
    {
        public int PermissionId { get; set; }
        public string SystemName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;

        /// <summary>
        /// هل الصلاحية موروثة من دور المستخدم؟
        /// </summary>
        public bool IsInherited { get; set; }

        /// <summary>
        /// هل الدور يمنح هذه الصلاحية أصلاً؟
        /// </summary>
        public bool InheritedAccess { get; set; }

        /// <summary>
        /// نوع التجاوز الفردي المطبق: "None" (موروث)، "Grant" (سماح فردي)، "Deny" (حظر فردي)
        /// </summary>
        public string OverrideType { get; set; } = "None";

        /// <summary>
        /// الصلاحية الفعلية النهائية للمستخدم بعد دمج الدور والاسثتناءات.
        /// </summary>
        public bool EffectiveAccess { get; set; }
    }
}
