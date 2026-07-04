// ============================================================
// AllDtos — DTOs المتنوعة
// ============================================================
// المسؤولية: تعريف DTOs المتنوعة.
// ============================================================
namespace DataAccess.DTOs
{
    /// <summary>
    /// سجل الضيف.
    /// </summary>
    public record GuestDto(int GuestId, string FullName, string? Organization, string? PhoneNumber, string? EmailAddress, string? Bio, string? Gender);

    /// <summary>
    /// يمثل ضيفاً مضافاً لحلقة معينة بكامل بياناته القابلة للتحرير
    /// <summary>
    /// سجل حلقة-ضيف.
    /// </summary>
    /// </summary>
    public record EpisodeGuestDto(
        int EpisodeGuestId,
        int GuestId,
        string FullName,
        string? Topic,
        TimeSpan? HostingTime,
        string? ClipNotes);
    /// <summary>
    /// سجل البرنامج.
    /// </summary>
    public record ProgramDto(int ProgramId, string ProgramName, string? Category, string? ProgramDescription);
    /// <summary>
    /// سجل الحلقة.
    /// </summary>
    public record EpisodeDto(
        int EpisodeId,
        int ProgramId,
        List<EpisodeGuestDto> Guests,
        List<EpisodeCorrespondentDto> Correspondents,
        List<EpisodeEmployeeDto> Employees,
        string EpisodeName,
        string? EpisodeDescription,
        DateTime? ScheduledDate,
        TimeSpan? BroadcastTime,
        string? SpecialNotes)
    {
        /// <summary>يدمج التاريخ والوقت في قيمة DateTime واحدة لحفظها في قاعدة البيانات</summary>
        public DateTime? ScheduledDateTime =>
            ScheduledDate.HasValue
                ? ScheduledDate.Value.Date + (BroadcastTime ?? TimeSpan.Zero)
                : null;
    };
    /// <summary>
    /// سجل المراسل.
    /// </summary>
    public record CorrespondentDto(int CorrespondentId, string FullName, string? PhoneNumber, string? AssignedLocations);
    /// <summary>
    /// سجل Today الحلقة.
    /// </summary>
    public record TodayEpisodeDto(
        int EpisodeId, string EpisodeName, string ProgramName,
        string GuestsDisplay,                                   // ✅ بدل string GuestName
        DateTime? ScheduledExecutionTime, string StatusText);
    /// <summary>
    /// سجل Active الضيف.
    /// </summary>
    public record ActiveGuestDto(int GuestId, string FullName, string? Organization, int EpisodeCount);

    /// <summary>
    /// سجل الضيف Display عنصر.
    /// </summary>
    public record GuestDisplayItem(int GuestId, string Name, string? Topic, TimeSpan? HostingTime);

    /// <summary>
    /// نتيجة تقرير الحلقات بفلتر التاريخ
    /// <summary>
    /// سجل التاريخ Range الحلقة.
    /// </summary>
    /// </summary>
    public record DateRangeEpisodeDto(
        int EpisodeId,
        string EpisodeName,
        string ProgramName,
        string GuestsDisplay,
        DateTime? ScheduledExecutionTime,
        string StatusText);

    /// <summary>
    /// تقرير الضيوف الأكثر ظهوراً
    /// <summary>
    /// سجل Top الضيف.
    /// </summary>
    /// </summary>
    public record TopGuestDto(
        int Rank,
        int GuestId,
        string FullName,
        string? Organization,
        int AppearanceCount,
        string? LastTopic,
        DateTime? LastAppearance);

    /// <summary>
    /// تقرير الحلقات الملغاة مع الأسباب
    /// <summary>
    /// سجل Cancelled الحلقة.
    /// </summary>
    /// </summary>
    public record CancelledEpisodeDto(
        int EpisodeId,
        string EpisodeName,
        string ProgramName,
        DateTime? ScheduledExecutionTime,
        string CancellationReason,
        string? CancelledBy,
        DateTime CancelledAt);

}
