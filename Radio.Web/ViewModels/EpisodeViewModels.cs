using DataAccess.DTOs;

namespace Radio.Web.ViewModels;

public class EpisodeListViewModel
{
    public List<ActiveEpisodeDto> Episodes { get; set; } = new();
    public List<ProgramDto> Programs { get; set; } = new();
    public string SearchTerm { get; set; } = "";
    public byte? StatusFilter { get; set; }
    public int? ProgramFilter { get; set; }
}

public class EpisodeDetailsViewModel
{
    public ActiveEpisodeDto Episode { get; set; } = default!;
    public List<PublishingRecordDto> PublishingRecords { get; set; } = new();
}

public class EpisodeEditViewModel
{
    public EpisodeDto Episode { get; set; } = default!;
    public string? StatusText { get; set; }
    public byte StatusId { get; set; }
    public List<ProgramDto> Programs { get; set; } = new();
    public List<GuestDto> Guests { get; set; } = new();
    public List<CorrespondentDto> Correspondents { get; set; } = new();
    public List<StaffRoleDto> StaffRoles { get; set; } = new();
    public List<EmployeeDto> Employees { get; set; } = new();
}

public class EpisodeEditFormModel
{
    public int EpisodeId { get; set; }
    public int ProgramId { get; set; }
    public string EpisodeName { get; set; } = string.Empty;
    public string? EpisodeDescription { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public TimeSpan? BroadcastTime { get; set; }
    public string? SpecialNotes { get; set; }

    // Nested collections
    public List<EpisodeGuestFormItem> Guests { get; set; } = new();
    public List<EpisodeCorrespondentFormItem> Correspondents { get; set; } = new();
    public List<EpisodeEmployeeFormItem> Employees { get; set; } = new();

    public EpisodeDto ToDto() => new(
        EpisodeId, ProgramId,
        Guests.Select(g => new EpisodeGuestDto(g.EpisodeGuestId, g.GuestId, g.FullName ?? "", g.Topic, g.HostingTime, g.ClipNotes)).ToList(),
        Correspondents.Select(c => new EpisodeCorrespondentDto(c.Id, c.CorrespondentId, c.FullName ?? "", c.Topic, c.HostingTime)).ToList(),
        Employees.Select(e => new EpisodeEmployeeDto(e.Id, e.EmployeeId, e.FullName, e.StaffRoleName)).ToList(),
        EpisodeName, EpisodeDescription, ScheduledDate, BroadcastTime, SpecialNotes);
}

public class EpisodeGuestFormItem
{
    public int EpisodeGuestId { get; set; }
    public int GuestId { get; set; }
    public string? FullName { get; set; }
    public string? Topic { get; set; }
    public TimeSpan? HostingTime { get; set; }
    public string? ClipNotes { get; set; }
}

public class EpisodeCorrespondentFormItem
{
    public int Id { get; set; }
    public int CorrespondentId { get; set; }
    public string? FullName { get; set; }
    public string? Topic { get; set; }
    public TimeSpan? HostingTime { get; set; }
}

public class EpisodeEmployeeFormItem
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string? FullName { get; set; }
    public string? StaffRoleName { get; set; }
}
