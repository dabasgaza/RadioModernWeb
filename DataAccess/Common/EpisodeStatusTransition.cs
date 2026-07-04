// ============================================================
// EpisodeStatusTransition — انتقال حالة الحلقة
// ============================================================
// المسؤولية: تعريف انتقال حالة الحلقة.
// ============================================================
using DataAccess.Services;

namespace DataAccess.Common;

/// <summary>
/// صنف انتقال حالة الحلقة.
/// </summary>
public static class EpisodeStatusTransition
{
    private static readonly Dictionary<byte, HashSet<byte>> s_validTransitions = new()
    {
        [EpisodeStatusValues.Planned] = [EpisodeStatusValues.Executed, EpisodeStatusValues.Cancelled],
        [EpisodeStatusValues.Executed] = [EpisodeStatusValues.Published, EpisodeStatusValues.Cancelled],
        [EpisodeStatusValues.Published] = [EpisodeStatusValues.WebsitePublished, EpisodeStatusValues.Cancelled],
        [EpisodeStatusValues.WebsitePublished] = [EpisodeStatusValues.Cancelled],
        [EpisodeStatusValues.Cancelled] = []
    };

    /// <summary>
    /// Is Valid.
    /// </summary>
    public static bool IsValid(byte fromStatus, byte toStatus)
    {
        if (fromStatus == toStatus) return false;
        return s_validTransitions.TryGetValue(fromStatus, out var allowed) && allowed.Contains(toStatus);
    }

    /// <summary>
    /// استرجاع الكل owed Transitions.
    /// </summary>
    public static IReadOnlySet<byte> GetAllowedTransitions(byte fromStatus)
    {
        return s_validTransitions.TryGetValue(fromStatus, out var allowed)
            ? allowed
            : [];
    }
}
