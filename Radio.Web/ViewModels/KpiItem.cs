namespace Radio.Web.ViewModels;

public class KpiItem
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string Icon { get; set; } = "info";
    public string? Color { get; set; }
    public string? IconBg { get; set; }
    public string? IconColor { get; set; }
    public string? Subtitle { get; set; }
    public string? Url { get; set; }
}
