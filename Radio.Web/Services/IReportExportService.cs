using DataAccess.DTOs;

using Radio.Web.ViewModels;

namespace Radio.Web.Services;

public interface IReportExportService
{
    Task<byte[]> ExportIndexToExcelAsync(ReportsViewModel vm, CancellationToken ct = default);
    Task<byte[]> ExportIndexToPdfAsync(ReportsViewModel vm, CancellationToken ct = default);
    Task<byte[]> ExportDateRangeToExcelAsync(List<DateRangeEpisodeDto> episodes, DateTime from, DateTime to, CancellationToken ct = default);
    Task<byte[]> ExportDateRangeToPdfAsync(List<DateRangeEpisodeDto> episodes, DateTime from, DateTime to, CancellationToken ct = default);
    Task<byte[]> ExportCancelledToExcelAsync(List<CancelledEpisodeDto> episodes, CancellationToken ct = default);
    Task<byte[]> ExportCancelledToPdfAsync(List<CancelledEpisodeDto> episodes, CancellationToken ct = default);
}
