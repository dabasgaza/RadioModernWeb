// ============================================================
// StatusHelper — الحالة
// ============================================================
// المسؤولية: تعريف الحالة.
// ============================================================
namespace Radio.Web.Helpers;

/// <summary>
/// StatusHelper: صنف StatusHelper.
/// <summary>
/// صنف StatusHelper.
/// </summary>
/// <summary>
/// صنف الحالة Helper.
/// </summary>
/// <summary>
/// صنف الحالة Helper.
/// </summary>
/// <summary>
/// صنف الحالة.
/// </summary>
/// <summary>
/// صنف الحالة.
/// </summary>
/// <summary>
/// صنف الحالة.
/// </summary>
/// </summary>
public static class StatusHelper
{
    /// <summary>
    /// معالجة Radio.Web.
    /// <summary>
    /// استرجاع الحالة Css Class.
    /// </summary>
    /// <summary>
    /// استرجاع الحالة Css Class.
    /// </summary>
    /// <summary>
    /// استرجاع الحالة Css Class.
    /// </summary>
    /// <summary>
    /// استرجاع الحالة Css Class.
    /// </summary>
    /// </summary>
    public static string GetStatusCssClass(byte statusId) => statusId switch
    {
        0 => "planned",
        1 => "executed",
        2 => "published",
        3 => "website-published",
        4 => "cancelled",
        _ => "planned"
    };

    /// <summary>
    /// معالجة Radio.Web.
    /// <summary>
    /// استرجاع الحالة Css Class.
    /// </summary>
    /// <summary>
    /// استرجاع الحالة Css Class.
    /// </summary>
    /// <summary>
    /// استرجاع الحالة Css Class.
    /// </summary>
    /// <summary>
    /// استرجاع الحالة Css Class.
    /// </summary>
    /// </summary>
    public static string GetStatusCssClass(string statusText) => statusText switch
    {
        "مجدولة" or "مخطط لها" => "planned",
        "تم التنفيذ" or "منفّذة" => "executed",
        "منشورة" or "منشورة رقمياً" => "published",
        "منشورة على الموقع" => "website-published",
        "ملغاة" => "cancelled",
        _ => "planned"
    };
}
