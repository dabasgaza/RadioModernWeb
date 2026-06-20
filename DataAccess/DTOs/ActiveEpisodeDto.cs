using DataAccess.Services;

namespace DataAccess.DTOs
{
    public class ActiveEpisodeDto
    {
        public int EpisodeId { get; init; }
        public int ProgramId { get; init; }
        public string? EpisodeName { get; init; }
        public string? ProgramName { get; init; }
        public string? EpisodeDescription { get; init; }
        public string? GuestsDisplay { get; set; }
        public DateTime? ScheduledExecutionTime { get; init; }
        public DateTime? ActualExecutionTime { get; init; }
        public string? StatusText { get; init; }
        public byte StatusId { get; init; }
        public string? SpecialNotes { get; init; }

        public bool CanMarkExecuted => StatusId == EpisodeStatus.Planned;
        public bool CanMarkPublished => StatusId == EpisodeStatus.Executed;
        public bool CanToggleWebsitePublish => (StatusId == EpisodeStatus.Executed || StatusId == EpisodeStatus.Published);
        public bool CanRevert => StatusId is EpisodeStatus.Executed or EpisodeStatus.Published or EpisodeStatus.WebsitePublished;
        public bool CanCancel => StatusId is EpisodeStatus.Planned or EpisodeStatus.Executed;
        public bool CanViewRecords => StatusId is EpisodeStatus.Executed or EpisodeStatus.Published or EpisodeStatus.WebsitePublished;

        public List<GuestDisplayItem> GuestItems { get; init; } = new List<GuestDisplayItem>();
        public List<EpisodeCorrespondentDto> CorrespondentItems { get; init; } = new List<EpisodeCorrespondentDto>();
        public List<EpisodeEmployeeDto> EmployeeItems { get; init; } = new List<EpisodeEmployeeDto>();
        public string? CancellationReason { get; set; }
        public bool IsSelected { get; set; }
    }

    /// <summary>مراسل مضاف لحلقة بكامل بياناته القابلة للتحرير</summary>
    public record EpisodeCorrespondentDto(int Id, int CorrespondentId, string FullName, string? Topic, TimeSpan? HostingTime);
    public record EpisodeEmployeeDto(int Id, int EmployeeId, string? FullName = null, string? StaffRoleName = null);
}
