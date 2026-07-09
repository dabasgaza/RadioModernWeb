using ClosedXML.Excel;
using DataAccess.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

using Radio.Web.ViewModels;

namespace Radio.Web.Services;

public class ReportExportService : IReportExportService
{
    public ReportExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> ExportIndexToExcelAsync(ReportsViewModel vm, CancellationToken ct = default)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("التقارير");

        ws.RightToLeft = true;
        ws.Cell(1, 1).Value = "التقارير والإحصائيات";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 16;

        int row = 3;
        ws.Cell(row, 1).Value = "إجمالي الحلقات";
        ws.Cell(row, 2).Value = vm.StatusStats.Values.Sum();

        row += 2;
        ws.Cell(row, 1).Value = "توزيع حالات الحلقات";
        ws.Cell(row, 1).Style.Font.Bold = true;
        row++;
        ws.Cell(row, 1).Value = "الحالة";
        ws.Cell(row, 2).Value = "العدد";
        ws.Range(row, 1, row, 2).Style.Font.Bold = true;
        foreach (var s in vm.StatusStats)
        {
            row++;
            ws.Cell(row, 1).Value = GetStatusDisplayName(s.Key);
            ws.Cell(row, 2).Value = s.Value;
        }

        row += 2;
        ws.Cell(row, 1).Value = "أبرز البرامج";
        ws.Cell(row, 1).Style.Font.Bold = true;
        row++;
        ws.Cell(row, 1).Value = "البرنامج";
        ws.Cell(row, 2).Value = "الفئة";
        ws.Cell(row, 3).Value = "إجمالي الحلقات";
        ws.Cell(row, 4).Value = "المنشورة";
        ws.Range(row, 1, row, 4).Style.Font.Bold = true;
        foreach (var p in vm.TopPrograms)
        {
            row++;
            ws.Cell(row, 1).Value = p.ProgramName;
            ws.Cell(row, 2).Value = p.Category ?? "عام";
            ws.Cell(row, 3).Value = p.TotalEpisodes;
            ws.Cell(row, 4).Value = p.PublishedEpisodes;
        }

        row += 2;
        ws.Cell(row, 1).Value = "أبرز الضيوف";
        ws.Cell(row, 1).Style.Font.Bold = true;
        row++;
        ws.Cell(row, 1).Value = "#";
        ws.Cell(row, 2).Value = "الاسم";
        ws.Cell(row, 3).Value = "الجهة";
        ws.Cell(row, 4).Value = "عدد الاستضافات";
        ws.Cell(row, 5).Value = "آخر ظهور";
        ws.Range(row, 1, row, 5).Style.Font.Bold = true;
        foreach (var g in vm.TopGuests)
        {
            row++;
            ws.Cell(row, 1).Value = g.Rank;
            ws.Cell(row, 2).Value = g.FullName;
            ws.Cell(row, 3).Value = g.Organization ?? "—";
            ws.Cell(row, 4).Value = g.AppearanceCount;
            ws.Cell(row, 5).Value = g.LastAppearance?.ToString("yyyy-MM-dd") ?? "—";
        }

        ws.Columns().AdjustToContents();
        return Task.FromResult(ToBytes(wb));
    }

    public Task<byte[]> ExportIndexToPdfAsync(ReportsViewModel vm, CancellationToken ct = default)
    {
        var total = vm.StatusStats.Values.Sum();
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontFamily("Segoe UI").FontSize(10));

                page.Header().Element(c => c
                    .AlignCenter()
                    .Text("التقارير والإحصائيات").Bold().FontSize(18));

                page.Content().Column(col =>
                {
                    col.Spacing(8);

                    col.Item().Text($"إجمالي الحلقات: {total}").Bold();

                    col.Item().Text("توزيع حالات الحلقات:").Bold();
                    foreach (var s in vm.StatusStats)
                        col.Item().PaddingLeft(10).Text($"  {GetStatusDisplayName(s.Key)}: {s.Value}");

                    if (vm.TopPrograms.Any())
                    {
                        col.Item().Text("أبرز البرامج:").Bold();
                        foreach (var p in vm.TopPrograms)
                            col.Item().PaddingLeft(10).Text($"  {p.ProgramName} — {p.TotalEpisodes} حلقة ({p.PublishedEpisodes} منشورة)");
                    }

                    if (vm.TopGuests.Any())
                    {
                        col.Item().Text("أبرز الضيوف:").Bold();
                        foreach (var g in vm.TopGuests)
                            col.Item().PaddingLeft(10).Text($"  #{g.Rank} {g.FullName} — {g.AppearanceCount} استضافة");
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("تاريخ التقرير: ");
                    text.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                });
            });
        });

        return Task.FromResult(doc.GeneratePdf());
    }

    public Task<byte[]> ExportDateRangeToExcelAsync(List<DateRangeEpisodeDto> episodes, DateTime from, DateTime to, CancellationToken ct = default)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("الحلقات حسب الفترة");
        ws.RightToLeft = true;

        ws.Cell(1, 1).Value = $"الحلقات من {from:yyyy-MM-dd} إلى {to:yyyy-MM-dd}";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;

        ws.Cell(3, 1).Value = "الحلقة";
        ws.Cell(3, 2).Value = "البرنامج";
        ws.Cell(3, 3).Value = "الضيوف";
        ws.Cell(3, 4).Value = "الموعد";
        ws.Cell(3, 5).Value = "الحالة";
        ws.Range(3, 1, 3, 5).Style.Font.Bold = true;

        int row = 4;
        foreach (var ep in episodes)
        {
            ws.Cell(row, 1).Value = ep.EpisodeName;
            ws.Cell(row, 2).Value = ep.ProgramName;
            ws.Cell(row, 3).Value = ep.GuestsDisplay;
            ws.Cell(row, 4).Value = ep.ScheduledExecutionTime?.ToString("yyyy-MM-dd HH:mm") ?? "—";
            ws.Cell(row, 5).Value = ep.StatusText;
            row++;
        }

        ws.Columns().AdjustToContents();
        return Task.FromResult(ToBytes(wb));
    }

    public Task<byte[]> ExportDateRangeToPdfAsync(List<DateRangeEpisodeDto> episodes, DateTime from, DateTime to, CancellationToken ct = default)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontFamily("Segoe UI").FontSize(10));

                page.Header().Element(c => c
                    .AlignCenter()
                    .Text($"الحلقات من {from:yyyy-MM-dd} إلى {to:yyyy-MM-dd}").Bold().FontSize(16));

                page.Content().Column(col =>
                {
                    col.Spacing(4);
                    foreach (var ep in episodes)
                    {
                        col.Item().Row(r =>
                        {
                            r.AutoItem().Text($"{ep.EpisodeName} — ").Bold();
                            r.AutoItem().Text($"{ep.ProgramName} | {ep.ScheduledExecutionTime?.ToString("yyyy-MM-dd HH:mm") ?? "—"} | {ep.StatusText}");
                        });
                    }
                });

                page.Footer().AlignCenter().Text($"تاريخ التقرير: {DateTime.Now:yyyy-MM-dd HH:mm}");
            });
        });

        return Task.FromResult(doc.GeneratePdf());
    }

    public Task<byte[]> ExportCancelledToExcelAsync(List<CancelledEpisodeDto> episodes, CancellationToken ct = default)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("الحلقات الملغاة");
        ws.RightToLeft = true;

        ws.Cell(1, 1).Value = "الحلقات الملغاة";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;

        ws.Cell(3, 1).Value = "الحلقة";
        ws.Cell(3, 2).Value = "البرنامج";
        ws.Cell(3, 3).Value = "الموعد";
        ws.Cell(3, 4).Value = "السبب";
        ws.Cell(3, 5).Value = "بواسطة";
        ws.Cell(3, 6).Value = "تاريخ الإلغاء";
        ws.Range(3, 1, 3, 6).Style.Font.Bold = true;

        int row = 4;
        foreach (var c in episodes)
        {
            ws.Cell(row, 1).Value = c.EpisodeName;
            ws.Cell(row, 2).Value = c.ProgramName;
            ws.Cell(row, 3).Value = c.ScheduledExecutionTime?.ToString("yyyy-MM-dd") ?? "—";
            ws.Cell(row, 4).Value = c.CancellationReason;
            ws.Cell(row, 5).Value = c.CancelledBy ?? "—";
            ws.Cell(row, 6).Value = c.CancelledAt.ToString("yyyy-MM-dd HH:mm");
            row++;
        }

        ws.Columns().AdjustToContents();
        return Task.FromResult(ToBytes(wb));
    }

    public Task<byte[]> ExportCancelledToPdfAsync(List<CancelledEpisodeDto> episodes, CancellationToken ct = default)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontFamily("Segoe UI").FontSize(10));

                page.Header().Element(c => c
                    .AlignCenter()
                    .Text("الحلقات الملغاة").Bold().FontSize(16));

                page.Content().Column(col =>
                {
                    col.Spacing(4);
                    foreach (var c in episodes)
                    {
                        col.Item().Row(r =>
                        {
                            r.AutoItem().Text($"{c.EpisodeName} — ").Bold();
                            r.AutoItem().Text($"{c.ProgramName} | {c.CancellationReason} | {c.CancelledAt:yyyy-MM-dd HH:mm}");
                        });
                    }
                });

                page.Footer().AlignCenter().Text($"تاريخ التقرير: {DateTime.Now:yyyy-MM-dd HH:mm}");
            });
        });

        return Task.FromResult(doc.GeneratePdf());
    }

    private static byte[] ToBytes(XLWorkbook wb)
    {
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static string GetStatusDisplayName(string s) => s switch
    {
        "Planned" => "مجدولة",
        "Executed" => "منفّذة",
        "Published" => "منشورة رقمياً",
        "WebsitePublished" => "منشورة على الموقع",
        "Cancelled" => "ملغاة",
        _ => s
    };
}
