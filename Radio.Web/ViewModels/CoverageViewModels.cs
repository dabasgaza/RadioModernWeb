using DataAccess.DTOs;

namespace Radio.Web.ViewModels;

public class CoverageListViewModel
{
    public List<CoverageDto> Coverages { get; set; } = new();
    public List<CorrespondentDto> Correspondents { get; set; } = new();
    public List<GuestDto> Guests { get; set; } = new();
}

public class CoverageEditViewModel
{
    public CoverageDto Coverage { get; set; } = new();
    public List<CorrespondentDto> Correspondents { get; set; } = new();
    public List<GuestDto> Guests { get; set; } = new();
}
