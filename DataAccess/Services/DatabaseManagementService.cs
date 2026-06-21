using DataAccess.Common;
using DataAccess.DTOs;
using DataAccess.Security;
using Domain.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace DataAccess.Services
{
    public class DatabaseManagementService : IDatabaseManagementService
    {
        private readonly IDbContextFactory<BroadcastWorkflowDBContext> _dbContextFactory;
        private readonly IConfiguration _configuration;
        private readonly CurrentSessionProvider _sessionProvider;
        private readonly SecureConfigurationProvider _secureConfigProvider;
        private readonly ILogger<DatabaseManagementService> _logger;

        public DatabaseManagementService(
            IDbContextFactory<BroadcastWorkflowDBContext> dbContextFactory,
            IConfiguration configuration,
            CurrentSessionProvider sessionProvider,
            SecureConfigurationProvider secureConfigProvider,
            ILogger<DatabaseManagementService> logger)
        {
            _dbContextFactory = dbContextFactory;
            _configuration = configuration;
            _sessionProvider = sessionProvider;
            _secureConfigProvider = secureConfigProvider;
            _logger = logger;
        }

        /// <summary>
        /// قراءة نص الاتصال الآمن — يدعم التشفير بـ DPAPI ومتغيرات البيئة.
        /// </summary>
        private string GetConnectionString()
        {
            var secureValue = _secureConfigProvider.GetSecureConnectionString(_configuration);
            if (!string.IsNullOrWhiteSpace(secureValue))
                return secureValue;

            // احتياطي آخر (لا يجب أن يُصل إليه في الظروف العادية)
            return "Server=.;Database=BroadcastWorkflowDB;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        private string GetDatabaseName(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            return builder.InitialCatalog;
        }

        public async Task<Result<string>> BackupDatabaseAsync(string? customBackupFolder = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionString = GetConnectionString();
                var dbName = GetDatabaseName(connectionString);

                // Determine folder path
                var backupFolder = customBackupFolder;
                if (string.IsNullOrEmpty(backupFolder))
                {
                    backupFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
                }

                if (!Directory.Exists(backupFolder))
                {
                    Directory.CreateDirectory(backupFolder);
                }

                var fileName = $"{dbName}_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                var fullPath = Path.Combine(backupFolder, fileName);

                // SQL command to backup
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync(cancellationToken);
                    // We check if compression is supported (SQL Server Standard/Developer/Enterprise support it)
                    // If not, we fall back to regular backup.
                    string backupQuery = $@"
                        BACKUP DATABASE [{dbName}] 
                        TO DISK = @BackupPath 
                        WITH FORMAT, INIT, NAME = @BackupName";

                    try
                    {
                        // Try with compression first
                        string compressionQuery = backupQuery + ", COMPRESSION";
                        using (var command = new SqlCommand(compressionQuery, connection))
                        {
                            command.Parameters.AddWithValue("@BackupPath", fullPath);
                            command.Parameters.AddWithValue("@BackupName", $"Radio Full Database Backup - {DateTime.Now}");
                            command.CommandTimeout = 300; // 5 minutes
                            await command.ExecuteNonQueryAsync(cancellationToken);
                        }
                    }
                    catch
                    {
                        // Fallback to no compression
                        using (var command = new SqlCommand(backupQuery, connection))
                        {
                            command.Parameters.AddWithValue("@BackupPath", fullPath);
                            command.Parameters.AddWithValue("@BackupName", $"Radio Full Database Backup - {DateTime.Now}");
                            command.CommandTimeout = 300;
                            await command.ExecuteNonQueryAsync(cancellationToken);
                        }
                    }
                }

                // Log the backup in database
                long fileSize = 0;
                if (File.Exists(fullPath))
                {
                    fileSize = new FileInfo(fullPath).Length;
                }

                await LogBackupAsync(fullPath, "Local", fileSize, "Success", cancellationToken: cancellationToken);

                return Result<string>.Success(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during processing");
                await LogBackupAsync(customBackupFolder ?? "N/A", "Local", 0, "Failed", ex.Message, cancellationToken);
                return Result<string>.Fail($"حدث خطأ أثناء عمل النسخة الاحتياطية: {ex.Message}");
            }
        }

        public async Task<Result> RestoreDatabaseAsync(string backupFilePath, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!File.Exists(backupFilePath))
                {
                    return Result.Fail("ملف النسخة الاحتياطية غير موجود.");
                }

                var connectionString = GetConnectionString();
                var dbName = GetDatabaseName(connectionString);

                // Build a connection string targeting master database to restore
                var masterBuilder = new SqlConnectionStringBuilder(connectionString)
                {
                    InitialCatalog = "master"
                };

                using (var connection = new SqlConnection(masterBuilder.ConnectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    // 1. Terminate active connections to the database to prevent locking errors
                    string setSingleUserQuery = $@"
                        ALTER DATABASE [{dbName}] 
                        SET SINGLE_USER 
                        WITH ROLLBACK IMMEDIATE;";

                    using (var cmd = new SqlCommand(setSingleUserQuery, connection))
                    {
                        await cmd.ExecuteNonQueryAsync(cancellationToken);
                    }

                    // 2. Perform the Restore
                    string restoreQuery = $@"
                        RESTORE DATABASE [{dbName}] 
                        FROM DISK = @BackupPath 
                        WITH REPLACE;";

                    using (var cmd = new SqlCommand(restoreQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@BackupPath", backupFilePath);
                        cmd.CommandTimeout = 600; // 10 minutes
                        await cmd.ExecuteNonQueryAsync(cancellationToken);
                    }

                    // 3. Set the database back to Multi-User mode
                    string setMultiUserQuery = $@"
                        ALTER DATABASE [{dbName}] 
                        SET MULTI_USER;";

                    using (var cmd = new SqlCommand(setMultiUserQuery, connection))
                    {
                        await cmd.ExecuteNonQueryAsync(cancellationToken);
                    }
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during processing");
                // Re-enable multi-user mode just in case of failure
                try
                {
                    var connectionString = GetConnectionString();
                    var dbName = GetDatabaseName(connectionString);
                    var masterBuilder = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" };
                    using (var connection = new SqlConnection(masterBuilder.ConnectionString))
                    {
                        await connection.OpenAsync(cancellationToken);
                        using (var cmd = new SqlCommand($"ALTER DATABASE [{dbName}] SET MULTI_USER;", connection))
                        {
                            await cmd.ExecuteNonQueryAsync(cancellationToken);
                        }
                    }
                }
                catch { /* Ignore fallback errors */ }

                return Result.Fail($"حدث خطأ أثناء استعادة النسخة الاحتياطية: {ex.Message}");
            }
        }

        public async Task<Result> InitializeDatabaseAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var context = await _dbContextFactory.CreateDbContextAsync();
                await context.Database.MigrateAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during processing");
                return Result.Fail($"حدث خطأ أثناء فحص/تهيئة قاعدة البيانات: {ex.Message}");
            }
        }

        public async Task<Result> ResetDatabaseAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionString = GetConnectionString();
                var dbName = GetDatabaseName(connectionString);
                var masterBuilder = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" };

                // 1. إسقاط قاعدة البيانات بأمان بعد قطع كل الاتصالات النشطة
                using (var connection = new SqlConnection(masterBuilder.ConnectionString))
                {
                    await connection.OpenAsync(cancellationToken);
                    string dropDbQuery = $@"
                        IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{dbName}')
                        BEGIN
                            ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                            DROP DATABASE [{dbName}];
                        END";
                    using (var cmd = new SqlCommand(dropDbQuery, connection))
                    {
                        await cmd.ExecuteNonQueryAsync(cancellationToken);
                    }
                }

                // 2. إنشاء قاعدة البيانات وتطبيق جميع الهجرات من الصفر
                using var context = await _dbContextFactory.CreateDbContextAsync();
                await context.Database.MigrateAsync(cancellationToken);

                // 3. ملء قاعدة البيانات بالبيانات الأولية والصلاحيات
                await Seeding.DbSeeder.SeedAsync(_dbContextFactory);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during processing");
                return Result.Fail($"حدث خطأ أثناء إعادة تعيين قاعدة البيانات: {ex.Message}");
            }
        }

        public async Task<Result<List<DatabaseBackupLog>>> GetBackupHistoryAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var context = await _dbContextFactory.CreateDbContextAsync();
                var list = await context.DatabaseBackupLogs
                    .AsNoTracking()
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync(cancellationToken);
                return Result<List<DatabaseBackupLog>>.Success(list);
            }
            catch (Exception ex) when (ex is Microsoft.Data.SqlClient.SqlException || ex is InvalidOperationException)
            {
                return Result<List<DatabaseBackupLog>>.Fail("جدول سجل النسخ الاحتياطي (DatabaseBackupLogs) غير موجود أو لم يُنشأ بعد. الرجاء الضغط على زر \"تهيئة قاعدة البيانات\" أولاً.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during processing");
                return Result<List<DatabaseBackupLog>>.Fail($"حدث خطأ أثناء جلب سجل النسخ الاحتياطي: {ex.Message}");
            }
        }

        public async Task<Result> CloudSyncBackupAsync(string localBackupPath, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!File.Exists(localBackupPath))
                {
                    return Result.Fail("ملف النسخة الاحتياطية المحلي غير موجود.");
                }

                var fileName = Path.GetFileName(localBackupPath);

                // Define a simulated Cloud backup folder for demo/local infrastructure purposes
                var cloudFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups", "CloudSimulation");
                if (!Directory.Exists(cloudFolder))
                {
                    Directory.CreateDirectory(cloudFolder);
                }

                var cloudPath = Path.Combine(cloudFolder, fileName);
                File.Copy(localBackupPath, cloudPath, true);

                // Update the log in database
                using var context = await _dbContextFactory.CreateDbContextAsync();
                var log = await context.DatabaseBackupLogs
                    .FirstOrDefaultAsync(x => x.BackupPath == localBackupPath, cancellationToken);

                if (log != null)
                {
                    log.CloudUrl = $"https://simulated-cloud.radio.local/backups/{fileName}";
                    log.BackupType = "Both";
                    log.UpdatedAt = DateTime.UtcNow;
                    await context.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    var newLog = new DatabaseBackupLog
                    {
                        BackupPath = localBackupPath,
                        BackupType = "Cloud",
                        FileSize = new FileInfo(localBackupPath).Length,
                        Status = "Success",
                        CloudUrl = $"https://simulated-cloud.radio.local/backups/{fileName}"
                    };
                    context.DatabaseBackupLogs.Add(newLog);
                    await context.SaveChangesAsync(cancellationToken);
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during processing");
                return Result.Fail($"حدث خطأ أثناء المزامنة السحابية: {ex.Message}");
            }
        }

        public async Task<Result> RunRetentionPolicyAsync(int retentionDays, CancellationToken cancellationToken = default)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
                using var context = await _dbContextFactory.CreateDbContextAsync();

                // Get logs older than cutoff
                var oldLogs = await context.DatabaseBackupLogs
                    .Where(x => x.CreatedAt < cutoffDate && x.Status == "Success")
                    .ToListAsync(cancellationToken);

                int deletedCount = 0;
                foreach (var log in oldLogs)
                {
                    try
                    {
                        if (File.Exists(log.BackupPath))
                        {
                            File.Delete(log.BackupPath);
                        }

                        // Also delete simulated cloud copy
                        var cloudSimPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups", "CloudSimulation", Path.GetFileName(log.BackupPath));
                        if (File.Exists(cloudSimPath))
                        {
                            File.Delete(cloudSimPath);
                        }

                        log.IsActive = false; // Soft delete
                        log.UpdatedAt = DateTime.UtcNow;
                        deletedCount++;
                    }
                    catch { /* Continue next */ }
                }

                if (deletedCount > 0)
                {
                    await context.SaveChangesAsync(cancellationToken);
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during processing");
                return Result.Fail($"حدث خطأ أثناء تطبيق سياسة الاحتفاظ: {ex.Message}");
            }
        }

        public async Task<DatabaseDashboardDto?> GetDatabaseDashboardAsync(CancellationToken cancellationToken = default)
        {
            long dataSize = 0, logSize = 0;
            int activeConnections = 0;
            string dbName = "";
            try
            {
                var connectionString = GetConnectionString();
                dbName = GetDatabaseName(connectionString);

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    using (var cmd = new SqlCommand(@"
                        SELECT 
                            SUM(CASE WHEN type_desc = 'ROWS' THEN size ELSE 0 END) * 8 * 1024,
                            SUM(CASE WHEN type_desc = 'LOG' THEN size ELSE 0 END) * 8 * 1024
                        FROM sys.database_files", connection))
                    {
                        cmd.CommandTimeout = 10;
                        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                        if (await reader.ReadAsync(cancellationToken))
                        {
                            dataSize = reader.GetInt64(0);
                            logSize = reader.GetInt64(1);
                        }
                    }

                    using (var cmd = new SqlCommand(@"
                        SELECT COUNT(*) FROM sys.dm_exec_sessions
                        WHERE database_id = DB_ID(@DbName) AND status = 'running'", connection))
                    {
                        cmd.Parameters.AddWithValue("@DbName", dbName);
                        cmd.CommandTimeout = 10;
                        activeConnections = (int)(await cmd.ExecuteScalarAsync(cancellationToken) ?? 0);
                    }
                }
            }
            catch { /* non-critical for dashboard */ }

            // Backup stats from DB
            int totalBackups = 0, backupsThisMonth = 0, failureCount = 0;
            double successRate = 100;
            DateTime? lastBackupAt = null;
            long lastBackupSize = 0;

            try
            {
                using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
                var now = DateTime.UtcNow;
                var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

                totalBackups = await context.DatabaseBackupLogs.CountAsync(cancellationToken);
                var successCount = await context.DatabaseBackupLogs.CountAsync(x => x.Status == "Success", cancellationToken);
                backupsThisMonth = await context.DatabaseBackupLogs
                    .CountAsync(x => x.CreatedAt >= monthStart, cancellationToken);
                failureCount = await context.DatabaseBackupLogs.CountAsync(x => x.Status == "Failed", cancellationToken);
                successRate = totalBackups > 0 ? (double)successCount / totalBackups * 100 : 100;

                var last = await context.DatabaseBackupLogs
                    .Where(x => x.Status == "Success")
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);
                if (last != null)
                {
                    lastBackupAt = last.CreatedAt;
                    lastBackupSize = last.FileSize;
                }
            }
            catch { /* backup log table may not exist yet */ }

            // Backup folder size
            long folderSize = 0;
            try
            {
                var backupFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
                if (Directory.Exists(backupFolder))
                {
                    folderSize = Directory.EnumerateFiles(backupFolder, "*", SearchOption.AllDirectories)
                        .Sum(f => new FileInfo(f).Length);
                }
            }
            catch { /* folder may not exist */ }

            // Config
            bool autoBackup = false, cloudSync = false;
            int retention = 30;
            try
            {
                autoBackup = _configuration.GetValue<bool>("DatabaseBackup:AutoBackup");
                cloudSync = _configuration.GetValue<bool>("DatabaseBackup:CloudSync");
                retention = _configuration.GetValue("DatabaseBackup:RetentionDays", 30);
            }
            catch { /* config keys may not exist */ }

            return new DatabaseDashboardDto(
                dataSize, logSize, lastBackupAt, lastBackupSize,
                totalBackups, successRate, backupsThisMonth,
                activeConnections, autoBackup, cloudSync,
                retention, folderSize, failureCount, dbName);
        }

        private async Task LogBackupAsync(string path, string type, long size, string status, string? error = null, CancellationToken cancellationToken = default)
        {
            try
            {
                using var context = await _dbContextFactory.CreateDbContextAsync();
                var log = new DatabaseBackupLog
                {
                    BackupPath = path,
                    BackupType = type,
                    FileSize = size,
                    Status = status,
                    ErrorMessage = error
                };
                context.DatabaseBackupLogs.Add(log);
                await context.SaveChangesAsync(cancellationToken);
            }
            catch { /* Avoid crashing on logging fail */ }
        }
    }
}
