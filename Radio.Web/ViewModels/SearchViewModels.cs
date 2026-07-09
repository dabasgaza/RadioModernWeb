namespace Radio.Web.ViewModels;

public class SearchViewModel
{
    public string Query { get; set; } = string.Empty;
    public List<SearchEpisodeItem> Episodes { get; set; } = new();
    public List<SearchProgramItem> Programs { get; set; } = new();
    public List<SearchGuestItem> Guests { get; set; } = new();
    public int TotalResults => Episodes.Count + Programs.Count + Guests.Count;
}

public class SearchEpisodeItem
{
    public int EpisodeId { get; set; }
    public string EpisodeName { get; set; } = string.Empty;
    public string? ProgramName { get; set; }
    public DateTime? ScheduledTime { get; set; }
    public string StatusText { get; set; } = string.Empty;
}

public class SearchProgramItem
{
    public int ProgramId { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int EpisodeCount { get; set; }
}

public class SearchGuestItem
{
    public int GuestId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Organization { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}
