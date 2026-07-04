// ============================================================
// CorrespondentCoverageDto — تغطية المراسل
// ============================================================
// المسؤولية: تعريف تغطية المراسل.
// ============================================================
namespace DataAccess.DTOs
{
    /// <summary>
    /// سجل تغطية المراسل.
    /// </summary>
    public record CorrespondentCoverageDto
    {
        public int CoverageId { get; set; }
        public string Topic { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
    }
}
