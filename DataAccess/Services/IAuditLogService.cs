using DataAccess.Common;
using Domain.Models;
using System.Threading;

namespace DataAccess.Services
{
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

    public class PagedAuditLogResult
    {
        public List<AuditLogDto> Items { get; set; } = [];
        public int TotalCount { get; set; }
    }

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

        Task<Result<List<User>>> GetAuditUsersAsync(CancellationToken cancellationToken = default);
    }
}
