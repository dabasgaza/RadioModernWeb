// ============================================================
// ErrorViewModel — ViewModel الخطأ
// ============================================================
// المسؤولية: تعريف ViewModel الخطأ.
// ============================================================
namespace Radio.Web.ViewModels;

/// <summary>
/// صنف ViewModel الخطأ.
/// </summary>
public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public string? ErrorMessage { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}

/// <summary>
/// صنف DashboardViewModel.
/// </summary>
public class DashboardViewModel
{
    public List<DataAccess.DTOs.TodayEpisodeDto> TodayEpisodes { get; set; } = new();
    public Dictionary<string, int> StatusStats { get; set; } = new();
    public List<DataAccess.DTOs.ActiveProgramDto> TopPrograms { get; set; } = new();
    public List<DataAccess.DTOs.TopGuestDto> TopGuests { get; set; } = new();
    public List<DataAccess.DTOs.CancelledEpisodeDto> CancelledEpisodes { get; set; } = new();
    public int TotalEpisodes { get; set; }
    public int TotalPrograms { get; set; }
}
