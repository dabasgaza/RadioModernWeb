// ============================================================
// EpisodeEditViewModelBuilder — بناء ViewModel التحرير
// ============================================================
// المسؤولية: تعريف بناء ViewModel التحرير.
// ============================================================
using DataAccess.DTOs;
using DataAccess.Services;
using Radio.Web.ViewModels;

namespace Radio.Web.Builders;

/// <summary>
/// تحرير الحلقة: صنف تحرير الحلقة.
/// <summary>
/// صنف بناء ViewModel التحرير.
/// </summary>
/// <summary>
/// صنف بناء ViewModel التحرير.
/// </summary>
/// <summary>
/// صنف بناء ViewModel التحرير.
/// </summary>
/// <summary>
/// صنف بناء ViewModel التحرير.
/// </summary>
/// <summary>
/// صنف بناء ViewModel التحرير.
/// </summary>
/// <summary>
/// صنف بناء ViewModel التحرير.
/// </summary>
/// </summary>
public class EpisodeEditViewModelBuilder
{
    private readonly ICachedLookupService _lookup;
    private readonly CancellationToken _cancellationToken;

    public EpisodeEditViewModelBuilder(ICachedLookupService lookup, CancellationToken cancellationToken)
    {
        _lookup = lookup;
        _cancellationToken = cancellationToken;
    }

    /// <summary>
    /// معالجة Radio.Web.
    /// <summary>
    /// From.
    /// </summary>
    /// <summary>
    /// From.
    /// </summary>
    /// <summary>
    /// From.
    /// </summary>
    /// <summary>
    /// From.
    /// </summary>
    /// </summary>
    public static EpisodeEditViewModelBuilder From(ICachedLookupService lookup, HttpContext? httpContext)
    {
        return new EpisodeEditViewModelBuilder(lookup, httpContext?.RequestAborted ?? default);
    }

    /// <summary>
    /// معالجة Radio.Web.
    /// <summary>
    /// بناء Async.
    /// </summary>
    /// <summary>
    /// بناء Async.
    /// </summary>
    /// <summary>
    /// بناء Async.
    /// </summary>
    /// <summary>
    /// بناء Async.
    /// </summary>
    /// </summary>
    public async Task<EpisodeEditViewModel> BuildAsync(EpisodeDto dto)
    {
        var programs = await _lookup.GetProgramsAsync(_cancellationToken);
        var guests = await _lookup.GetGuestsAsync(_cancellationToken);
        var correspondents = await _lookup.GetCorrespondentsAsync(_cancellationToken);
        var staffRoles = await _lookup.GetStaffRolesAsync(_cancellationToken);
        var employees = await _lookup.GetEmployeesAsync(_cancellationToken);

        return new EpisodeEditViewModel
        {
            Episode = dto,
            Programs = programs,
            Guests = guests,
            Correspondents = correspondents,
            StaffRoles = staffRoles,
            Employees = employees
        };
    }
}
