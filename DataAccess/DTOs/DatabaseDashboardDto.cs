namespace DataAccess.DTOs;

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
