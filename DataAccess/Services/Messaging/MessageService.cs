// ============================================================
// MessageService — الرسائل
// ============================================================
// المسؤولية: تعريف الرسائل.
// ============================================================
namespace DataAccess.Services.Messaging
{
    /// <summary>
    /// واجهة I الرسالة.
    /// </summary>
    public interface IMessageService
    {
        void ShowSuccess(string message, string title = "نجاح");
        void ShowError(string message, string title = "خطأ");
        void ShowWarning(string message, string title = "تحذير");
        void ShowInfo(string message, string title = "معلومة");

        // 👈 رسالة التأكيد ترجع Task<bool> لأنها توقف التنفيذ لانتظار رد المستخدم
        Task<bool> ShowConfirmationAsync(string message, string title = "تأكيد", CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// نظام الإشعارات المركزي (Ambient Context)
    /// <summary>
    /// صنف الرسائل.
    /// </summary>
    /// </summary>
    public static class MessageService
    {
        // الـ Default هو Null Object لمنع انهيار النظام إذا نسينا التهيئة
        public static IMessageService Current { get; private set; } = new NullMessageService();

        /// <summary>
        /// تهيئة الرسائل.
        /// </summary>
        public static void Initialize(IMessageService service)
        {
            Current = service ?? throw new ArgumentNullException(nameof(service));
        }
    }

    /// <summary>
    /// تطبيق الـ Null Object Pattern
    /// <summary>
    /// صنف Null الرسالة.
    /// </summary>
    /// </summary>
    public class NullMessageService : IMessageService
    {
        /// <summary>
        /// Show Success.
        /// </summary>
        public void ShowSuccess(string message, string title = "نجاح") { }
        /// <summary>
        /// Show Error.
        /// </summary>
        public void ShowError(string message, string title = "خطأ") { }
        /// <summary>
        /// Show Warning.
        /// </summary>
        public void ShowWarning(string message, string title = "تحذير") { }
        /// <summary>
        /// Show معلومات.
        /// </summary>
        public void ShowInfo(string message, string title = "معلومة") { }
        /// <summary>
        /// Show Confirmation Async.
        /// </summary>
        public Task<bool> ShowConfirmationAsync(string message, string title = "تأكيد", CancellationToken cancellationToken = default) => Task.FromResult(false);
    }

}
