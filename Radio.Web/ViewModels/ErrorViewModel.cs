namespace Radio.Web.ViewModels;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public string? ErrorMessage { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}

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
