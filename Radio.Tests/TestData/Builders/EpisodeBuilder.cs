using DataAccess.DTOs;
using Domain.Models;

namespace Radio.Tests.TestData.Builders;

public class EpisodeBuilder
{
    private int _episodeId;
    private int _programId = 1;
    private string _name = "Test Episode";
    private string? _description;
    private byte _statusId;
    private DateTime? _scheduledTime = DateTime.UtcNow.AddDays(1);
    private string? _specialNotes = null;
    private readonly List<EpisodeGuest> _guests = [];
    private readonly List<EpisodeEmployee> _employees = [];
    private readonly List<EpisodeCorrespondent> _correspondents = [];

    public EpisodeBuilder WithId(int id) { _episodeId = id; return this; }
    public EpisodeBuilder WithProgram(int programId) { _programId = programId; return this; }
    public EpisodeBuilder WithName(string name) { _name = name; return this; }
    public EpisodeBuilder WithDescription(string? desc) { _description = desc; return this; }
    public EpisodeBuilder WithStatus(byte status) { _statusId = status; return this; }
    public EpisodeBuilder AsPlanned() => WithStatus(0);
    public EpisodeBuilder AsExecuted() => WithStatus(1);
    public EpisodeBuilder AsPublished() => WithStatus(2);
    public EpisodeBuilder AsWebsitePublished() => WithStatus(3);
    public EpisodeBuilder AsCancelled() => WithStatus(4);

    public Episode Build() => new()
    {
        EpisodeId = _episodeId,
        ProgramId = _programId,
        EpisodeName = _name,
        EpisodeDescription = _description,
        StatusId = _statusId,
        ScheduledExecutionTime = _scheduledTime,
        SpecialNotes = _specialNotes,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    public static EpisodeDto CreateDto(int programId = 1, string name = "New Episode",
        DateTime? scheduledDate = null, TimeSpan? broadcastTime = null)
        => new(
            EpisodeId: 0,
            ProgramId: programId,
            Guests: [],
            Correspondents: [],
            Employees: [],
            EpisodeName: name,
            EpisodeDescription: null,
            ScheduledDate: scheduledDate ?? DateTime.UtcNow.AddDays(1),
            BroadcastTime: broadcastTime ?? TimeSpan.FromHours(10),
            SpecialNotes: null
        );
}
