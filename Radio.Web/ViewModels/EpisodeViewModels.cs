// ============================================================
// EpisodeViewModels — الحلقات
// ============================================================
// المسؤولية: تعريف الحلقات.
// ============================================================
using DataAccess.DTOs;

namespace Radio.Web.ViewModels;

/// <summary>
/// صنف الحلقة قائمة.
/// </summary>
public class EpisodeListViewModel
{
    public List<ActiveEpisodeDto> Episodes { get; set; } = new();
    public List<ProgramDto> Programs { get; set; } = new();
    public string SearchTerm { get; set; } = string.Empty;
    public byte? StatusFilter { get; set; }
    public int? ProgramFilter { get; set; }
}

/// <summary>
/// صنف الحلقة Details.
/// </summary>
public class EpisodeDetailsViewModel
{
    public ActiveEpisodeDto Episode { get; set; } = default!;
    public List<PublishingRecordDto> PublishingRecords { get; set; } = new();
}

/// <summary>
/// صنف تحرير الحلقة.
/// </summary>
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

/// <summary>
/// صنف الحلقة Edit Form.
/// </summary>
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

    /// <summary>
    /// To Dto.
    /// </summary>
    public EpisodeDto ToDto() => new(
        EpisodeId, ProgramId,
        Guests.Select(g => new EpisodeGuestDto(g.EpisodeGuestId, g.GuestId, g.FullName ?? string.Empty, g.Topic, g.HostingTime, g.ClipNotes)).ToList(),
        Correspondents.Select(c => new EpisodeCorrespondentDto(c.Id, c.CorrespondentId, c.FullName ?? string.Empty, c.Topic, c.HostingTime)).ToList(),
        Employees.Select(e => new EpisodeEmployeeDto(e.Id, e.EmployeeId, e.FullName, e.StaffRoleName)).ToList(),
        EpisodeName, EpisodeDescription, ScheduledDate, BroadcastTime, SpecialNotes);
}

/// <summary>
/// صنف Status Badge General.
/// </summary>
public class StatusBadgeViewModel
{
    public string Type { get; set; } = "episode";
    public string Status { get; set; } = "planned";
    public string Label { get; set; } = string.Empty;
    public string? IconOverride { get; set; }
}

/// <summary>
/// صنف الحلقة الضيف Form عنصر.
/// </summary>
public class EpisodeGuestFormItem
{
    public int EpisodeGuestId { get; set; }
    public int GuestId { get; set; }
    public string? FullName { get; set; }
    public string? Topic { get; set; }
    public TimeSpan? HostingTime { get; set; }
    public string? ClipNotes { get; set; }
}

/// <summary>
/// صنف الحلقة المراسل Form عنصر.
/// </summary>
public class EpisodeCorrespondentFormItem
{
    public int Id { get; set; }
    public int CorrespondentId { get; set; }
    public string? FullName { get; set; }
    public string? Topic { get; set; }
    public TimeSpan? HostingTime { get; set; }
}

/// <summary>
/// صنف الحلقة الموظف Form عنصر.
/// </summary>
public class EpisodeEmployeeFormItem
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string? FullName { get; set; }
    public string? StaffRoleName { get; set; }
}
