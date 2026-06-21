using DataAccess.Common;
using DataAccess.DTOs;
using Domain.Models;
using System.Threading;

namespace DataAccess.Services
{
    public interface IDatabaseManagementService
    {
        Task<Result<string>> BackupDatabaseAsync(string? customBackupFolder = null, CancellationToken cancellationToken = default);
        Task<Result> RestoreDatabaseAsync(string backupFilePath, CancellationToken cancellationToken = default);
        Task<Result> InitializeDatabaseAsync(CancellationToken cancellationToken = default);
        Task<Result> ResetDatabaseAsync(CancellationToken cancellationToken = default);
        Task<Result<List<DatabaseBackupLog>>> GetBackupHistoryAsync(CancellationToken cancellationToken = default);
        Task<Result> CloudSyncBackupAsync(string localBackupPath, CancellationToken cancellationToken = default);
        Task<Result> RunRetentionPolicyAsync(int retentionDays, CancellationToken cancellationToken = default);
        Task<DatabaseDashboardDto?> GetDatabaseDashboardAsync(CancellationToken cancellationToken = default);
    }
}
