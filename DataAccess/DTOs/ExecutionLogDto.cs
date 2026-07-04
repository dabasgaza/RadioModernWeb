// ============================================================
// ExecutionLogDto — سجل التنفيذ
// ============================================================
// المسؤولية: تعريف سجل التنفيذ.
// ============================================================
namespace DataAccess.DTOs
{
    /// <summary>
    /// سجل سجل التنفيذ.
    /// </summary>
    public record ExecutionLogDto
    {
        public int ExecutionLogId { get; set; }
        public int EpisodeId { get; set; }
        public int ExecutedByUserId { get; set; }
        public string ExecutionNotes { get; set; } = string.Empty;
        public string IssuesEncountered { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
    }
}
