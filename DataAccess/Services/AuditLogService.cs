using DataAccess.Common;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace DataAccess.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IDbContextFactory<BroadcastWorkflowDBContext> _dbContextFactory;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(
            IDbContextFactory<BroadcastWorkflowDBContext> dbContextFactory,
            ILogger<AuditLogService> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<Result<PagedAuditLogResult>> GetFilteredAuditLogsAsync(
            string? tableName = null,
            int? userId = null,
            string? action = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int page = 1,
            int pageSize = 100,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var context = await _dbContextFactory.CreateDbContextAsync();

                var logsQuery = context.AuditLogs.AsNoTracking().AsQueryable();

                if (!string.IsNullOrEmpty(tableName))
                    logsQuery = logsQuery.Where(x => x.TableName == tableName);

                if (userId.HasValue)
                    logsQuery = logsQuery.Where(x => x.UserId == userId.Value);

                if (!string.IsNullOrEmpty(action))
                    logsQuery = logsQuery.Where(x => x.Action == action);

                if (fromDate.HasValue)
                    logsQuery = logsQuery.Where(x => x.ChangedAt >= fromDate.Value);

                if (toDate.HasValue)
                {
                    var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);
                    logsQuery = logsQuery.Where(x => x.ChangedAt <= endOfDay);
                }

                var totalCount = await logsQuery.CountAsync(cancellationToken);

                var items = await (from log in logsQuery.OrderByDescending(x => x.ChangedAt).Skip((page - 1) * pageSize).Take(pageSize)
                                   join u in context.Users.AsNoTracking()
                                       on log.UserId equals u.UserId into userJoin
                                   from u in userJoin.DefaultIfEmpty()
                                   select new AuditLogDto
                                   {
                                       AuditLogId = log.AuditLogId,
                                       TableName = log.TableName,
                                       RecordId = log.RecordId,
                                       Action = log.Action,
                                       OldValues = log.OldValues,
                                       NewValues = log.NewValues,
                                       Reason = log.Reason,
                                       UserId = log.UserId,
                                       Username = u != null ? u.Username : "غير معروف",
                                       UserFullName = u != null ? u.FullName : "غير معروف",
                                       ChangedAt = log.ChangedAt
                                   })
                                   .ToListAsync(cancellationToken);

                return Result<PagedAuditLogResult>.Success(new PagedAuditLogResult
                {
                    Items = items,
                    TotalCount = totalCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during processing");
                return Result<PagedAuditLogResult>.Fail($"حدث خطأ أثناء جلب سجل العمليات: {ex.Message}");
            }
        }

        public async Task<Result<List<User>>> GetAuditUsersAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var context = await _dbContextFactory.CreateDbContextAsync();
                // ✅ Select الحقول المطلوبة فقط بدلاً من جلب الكيان كاملاً
                var users = await context.Users
                    .AsNoTracking()
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.FullName)
                    .Select(u => new User { UserId = u.UserId, FullName = u.FullName, Username = u.Username })
                    .ToListAsync(cancellationToken);
                return Result<List<User>>.Success(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during processing");
                return Result<List<User>>.Fail($"حدث خطأ أثناء جلب المستخدمين: {ex.Message}");
            }
        }
    }
}
