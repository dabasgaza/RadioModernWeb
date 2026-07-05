// ============================================================
// AppPermissions — الصلاحيات النظامية المحدثة
// ============================================================
// المسؤولية: تعريف الصلاحيات بنمط Module.Resource.Action.
// ============================================================
namespace DataAccess.Common
{
    [AttributeUsage(AttributeTargets.Field)]
    public class PermissionInfoAttribute(string displayName, string module) : Attribute
    {
        public string DisplayName { get; } = displayName;
        public string Module { get; } = module;
    }

    /// <summary>
    /// يحتوي على كافة ثوابت الصلاحيات لضمان عدم وجود أخطاء إملائية في الكود.
    /// الصلاحيات مصممة بالنمط الموحد: Module.Resource.Action
    /// </summary>
    public static class AppPermissions
    {
        // ─── الحلقات ───
        [PermissionInfo("عرض الحلقات", "الحلقات")]
        public const string EpisodesView = "Episodes.View";
        [PermissionInfo("إضافة حلقة", "الحلقات")]
        public const string EpisodesCreate = "Episodes.Create";
        [PermissionInfo("تعديل حلقة", "الحلقات")]
        public const string EpisodesEdit = "Episodes.Edit";
        [PermissionInfo("حذف حلقة", "الحلقات")]
        public const string EpisodesDelete = "Episodes.Delete";

        // ─── التنفيذ الفني ───
        [PermissionInfo("عرض سجلات التنفيذ", "التنفيذ الفني")]
        public const string ExecutionView = "Execution.View";
        [PermissionInfo("تسجيل التنفيذ الفني", "التنفيذ الفني")]
        public const string ExecutionExecute = "Execution.Execute";
        [PermissionInfo("تعديل بيانات التنفيذ", "التنفيذ الفني")]
        public const string ExecutionEdit = "Execution.Edit";
        [PermissionInfo("إلغاء التنفيذ", "التنفيذ الفني")]
        public const string ExecutionCancel = "Execution.Cancel";

        // ─── النشر الرقمي ───
        [PermissionInfo("عرض سجلات السوشيال", "النشر الرقمي")]
        public const string SocialPublishingView = "SocialPublishing.View";
        [PermissionInfo("النشر الرقمي", "النشر الرقمي")]
        public const string SocialPublishingPublish = "SocialPublishing.Publish";
        [PermissionInfo("تعديل نشر السوشيال", "النشر الرقمي")]
        public const string SocialPublishingEdit = "SocialPublishing.Edit";
        [PermissionInfo("حذف سجل السوشيال", "النشر الرقمي")]
        public const string SocialPublishingDelete = "SocialPublishing.Delete";

        // ─── نشر الموقع ───
        [PermissionInfo("عرض سجلات نشر الموقع", "نشر الموقع")]
        public const string WebsitePublishingView = "WebsitePublishing.View";
        [PermissionInfo("نشر الموقع", "نشر الموقع")]
        public const string WebsitePublishingPublish = "WebsitePublishing.Publish";
        [PermissionInfo("تعديل نشر الموقع", "نشر الموقع")]
        public const string WebsitePublishingEdit = "WebsitePublishing.Edit";
        [PermissionInfo("حذف سجل نشر الموقع", "نشر الموقع")]
        public const string WebsitePublishingDelete = "WebsitePublishing.Delete";

        // ─── البرامج ───
        [PermissionInfo("عرض البرامج", "البرامج")]
        public const string ProgramsView = "Programs.View";
        [PermissionInfo("إدارة البرامج", "البرامج")]
        public const string ProgramsManage = "Programs.Manage";

        // ─── الضيوف ───
        [PermissionInfo("عرض الضيوف", "الضيوف")]
        public const string GuestsView = "Guests.View";
        [PermissionInfo("إدارة الضيوف", "الضيوف")]
        public const string GuestsManage = "Guests.Manage";

        // ─── التنسيق الميداني ───
        [PermissionInfo("عرض التنسيق الميداني", "التنسيق")]
        public const string CoordinationView = "Coordination.View";
        [PermissionInfo("إدارة التنسيق الميداني", "التنسيق")]
        public const string CoordinationManage = "Coordination.Manage";

        // ─── طاقم العمل ───
        [PermissionInfo("عرض طاقم العمل", "طاقم العمل")]
        public const string StaffView = "Staff.View";
        [PermissionInfo("إدارة طاقم العمل", "طاقم العمل")]
        public const string StaffManage = "Staff.Manage";

        // ─── التقارير ───
        [PermissionInfo("عرض التقارير", "التقارير")]
        public const string ReportsView = "Reports.View";

        // ─── النظام وقاعدة البيانات ───
        [PermissionInfo("إدارة قاعدة البيانات", "النظام")]
        public const string SystemDatabaseManage = "System.DatabaseManage";
        [PermissionInfo("عرض سجلات التدقيق", "النظام")]
        public const string SystemViewAuditLogs = "System.ViewAuditLogs";

        // ─── إدارة المستخدمين والأدوار ───
        [PermissionInfo("إدارة المستخدمين", "المستخدمين والأدوار")]
        public const string AdminUsersManage = "Admin.UsersManage";
        [PermissionInfo("إدارة الأدوار", "المستخدمين والأدوار")]
        public const string AdminRolesManage = "Admin.RolesManage";


        // ════════════════════════════════════════════════════════════════════
        //  ثوابت التوافق القديمة (Backward Compatibility Map)
        //  مخططة للقيم الجديدة لتفادي كسر الأكواد والمحركات القائمة حالياً
        // ════════════════════════════════════════════════════════════════════
        [Obsolete("استخدم EpisodesView")]
        public const string EpisodeView = EpisodesView;
        [Obsolete("استخدم EpisodesCreate")]
        public const string EpisodeManage = EpisodesCreate;
        [Obsolete("استخدم EpisodesEdit")]
        public const string EpisodeEdit = EpisodesEdit;
        [Obsolete("استخدم EpisodesDelete")]
        public const string EpisodeDelete = EpisodesDelete;

        [Obsolete("استخدم ExecutionExecute")]
        public const string EpisodeExecute = ExecutionExecute;
        [Obsolete("استخدم ExecutionCancel")]
        public const string EpisodeRevert = ExecutionCancel;

        [Obsolete("استخدم SocialPublishingPublish")]
        public const string EpisodePublish = SocialPublishingPublish;
        [Obsolete("استخدم WebsitePublishingPublish")]
        public const string EpisodeWebPublish = WebsitePublishingPublish;
        [Obsolete("استخدم SocialPublishingView")]
        public const string PublishingRecordView = SocialPublishingView;

        [Obsolete("استخدم ProgramsManage")]
        public const string ProgramManage = ProgramsManage;
        [Obsolete("استخدم ProgramsView")]
        public const string ProgramView = ProgramsView;

        [Obsolete("استخدم GuestsManage")]
        public const string GuestManage = GuestsManage;
        [Obsolete("استخدم GuestsView")]
        public const string GuestView = GuestsView;



        [Obsolete("استخدم ReportsView")]
        public const string ViewReports = ReportsView;

        [Obsolete("استخدم SystemDatabaseManage")]
        public const string DatabaseManage = SystemDatabaseManage;
        [Obsolete("استخدم SystemDatabaseManage")]
        public const string DatabaseView = SystemDatabaseManage;
        [Obsolete("استخدم SystemViewAuditLogs")]
        public const string ViewAuditLogs = SystemViewAuditLogs;

        [Obsolete("استخدم AdminUsersManage")]
        public const string UserManage = AdminUsersManage;
        [Obsolete("استخدم AdminUsersManage")]
        public const string UserView = AdminUsersManage;

        private static readonly System.Collections.Generic.Dictionary<string, string> PermissionNormalizationMap = new(System.StringComparer.OrdinalIgnoreCase)
        {
            { "GUEST_VIEW", "Guests.View" },
            { "GUEST_MANAGE", "Guests.Manage" },
            { "EPISODE_VIEW", "Episodes.View" },
            { "EPISODE_MANAGE", "Episodes.Create" },
            { "EPISODE_EDIT", "Episodes.Edit" },
            { "EPISODE_DELETE", "Episodes.Delete" },
            { "EPISODE_EXECUTE", "Execution.Execute" },
            { "EPISODE_PUBLISH", "SocialPublishing.Publish" },
            { "EPISODE_WEB_PUBLISH", "WebsitePublishing.Publish" },
            { "EPISODE_REVERT", "Execution.Cancel" },
            { "PUBLISHING_RECORD_VIEW", "SocialPublishing.View" },
            { "PROGRAM_VIEW", "Programs.View" },
            { "PROGRAM_MANAGE", "Programs.Manage" },
            { "CORR_VIEW", "Coordination.View" },
            { "CORR_MANAGE", "Coordination.Manage" },
            { "STAFF_VIEW", "Staff.View" },
            { "STAFF_MANAGE", "Staff.Manage" },
            { "VIEW_REPORTS", "Reports.View" },
            { "DATABASE_VIEW", "System.DatabaseManage" },
            { "DATABASE_MANAGE", "System.DatabaseManage" },
            { "VIEW_AUDIT_LOGS", "System.ViewAuditLogs" },
            { "USER_VIEW", "Admin.UsersManage" },
            { "USER_MANAGE", "Admin.UsersManage" }
        };

        public static string Normalize(string permission)
        {
            if (System.String.IsNullOrWhiteSpace(permission)) return permission;
            return PermissionNormalizationMap.TryGetValue(permission, out var normalized) ? normalized : permission;
        }
    }
}
