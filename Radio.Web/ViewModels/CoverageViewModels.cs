// ============================================================
// CoverageViewModels — التغطية
// ============================================================
// المسؤولية: تعريف التغطية.
// ============================================================
using DataAccess.DTOs;

namespace Radio.Web.ViewModels;

/// <summary>
/// صنف التغطية قائمة.
/// </summary>
public class CoverageListViewModel
{
    public List<CoverageDto> Coverages { get; set; } = new();
    public List<CorrespondentDto> Correspondents { get; set; } = new();
    public List<GuestDto> Guests { get; set; } = new();
}

/// <summary>
/// صنف التغطية Edit.
/// </summary>
public class CoverageEditViewModel
{
    public CoverageDto Coverage { get; set; } = new();
    public List<CorrespondentDto> Correspondents { get; set; } = new();
    public List<GuestDto> Guests { get; set; } = new();
}
