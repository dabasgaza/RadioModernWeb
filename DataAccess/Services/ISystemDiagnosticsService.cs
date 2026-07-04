// ============================================================
// ISystemDiagnosticsService — I النظام Diagnostics
// ============================================================
// المسؤولية: تعريف I النظام Diagnostics.
// ============================================================
using DataAccess.Common;

namespace DataAccess.Services
{
    /// <summary>
    /// صنف Diagnostic سجل.
    /// </summary>
    public class DiagnosticLogDto
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } = "Information";
        public string Message { get; set; } = string.Empty;
        public string? Exception { get; set; }
        public string? SourceContext { get; set; }
        public string? Sql { get; set; }
        public double? DurationMs { get; set; }
        public bool IsSlowQuery { get; set; }
    }

    /// <summary>
    /// صنف Diagnostics Summary.
    /// </summary>
    public class DiagnosticsSummaryDto
    {
        public int TotalLogs { get; set; }
        public int TotalErrors { get; set; }
        public int TotalWarnings { get; set; }
        public int TotalQueries { get; set; }
        public int SlowQueriesCount { get; set; }
        public double AverageQueryTimeMs { get; set; }
    }

    /// <summary>
    /// واجهة I النظام Diagnostics.
    /// </summary>
    public interface ISystemDiagnosticsService
    {
        Task<Result<List<DiagnosticLogDto>>> GetLogsAsync(string? level = null, string? searchTerm = null, int count = 200, CancellationToken cancellationToken = default);
        Task<Result<DiagnosticsSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default);
        Task<Result<List<DiagnosticLogDto>>> GetSqlPerformanceLogsAsync(int count = 100, CancellationToken cancellationToken = default);
        Task<Result> ClearLogsAsync(CancellationToken cancellationToken = default);
    }
}
