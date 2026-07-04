// ============================================================
// IAuditLogService — I التدقيق سجل
// ============================================================
// المسؤولية: تعريف I التدقيق سجل.
// ============================================================
using DataAccess.Common;
using Domain.Identity;

namespace DataAccess.Services
{
    /// <summary>
    /// صنف سجل التدقيق.
    /// </summary>
    public class AuditLogDto
    {
        public int AuditLogId { get; set; }
        public string TableName { get; set; } = null!;
        public int? RecordId { get; set; }
        public string Action { get; set; } = null!;
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? Reason { get; set; }
        public int? UserId { get; set; }
        public string Username { get; set; } = "غير معروف";
        public string UserFullName { get; set; } = "غير معروف";
        public DateTime ChangedAt { get; set; }
    }

    /// <summary>
    /// صنف مقسم التدقيق سجل النتيجة.
    /// </summary>
    public class PagedAuditLogResult
    {
        public List<AuditLogDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// واجهة I التدقيق سجل.
    /// </summary>
    public interface IAuditLogService
    {
        Task<Result<PagedAuditLogResult>> GetFilteredAuditLogsAsync(
            string? tableName = null,
            int? userId = null,
            string? action = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int page = 1,
            int pageSize = 100,
            CancellationToken cancellationToken = default);

        Task<Result<List<ApplicationUser>>> GetAuditUsersAsync(CancellationToken cancellationToken = default);
    }
}
