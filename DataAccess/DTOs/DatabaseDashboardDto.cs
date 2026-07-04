// ============================================================
// DatabaseDashboardDto — لوحة تحكم قاعدة البيانات
// ============================================================
// المسؤولية: تعريف لوحة تحكم قاعدة البيانات.
// ============================================================
namespace DataAccess.DTOs;

/// <summary>
/// سجل لوحة تحكم قاعدة البيانات.
/// </summary>
public record DatabaseDashboardDto(
    long DatabaseSizeBytes,
    long DatabaseLogSizeBytes,
    DateTime? LastBackupAt,
    long LastBackupSizeBytes,
    int TotalBackups,
    double SuccessRate,
    int BackupsThisMonth,
    int ActiveConnections,
    bool IsAutoBackupEnabled,
    bool IsCloudSyncEnabled,
    int RetentionDays,
    long BackupFolderSizeBytes,
    int FailureCount,
    string DatabaseName
);
