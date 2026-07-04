// ============================================================
// PlatformPublishDtoValidator — التحقق من نشر المنصة
// ============================================================
// المسؤولية: تعريف التحقق من نشر المنصة.
// ============================================================
using DataAccess.DTOs;
using FluentValidation;

namespace DataAccess.Validation.Validators;

/// <summary>
/// صنف التحقق من نشر المنصة.
/// </summary>
public class PlatformPublishDtoValidator : AbstractValidator<PlatformPublishDto>
{
    public PlatformPublishDtoValidator()
    {
        RuleFor(x => x.PlatformId)
            .GreaterThan(0).WithMessage("يرجى اختيار منصة نشر.");

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("رابط النشر مطلوب.")
            .Must(BeValidUrl).WithMessage("رابط النشر غير صالح.");
    }

    /// <summary>
    /// Be Valid Url.
    /// </summary>
    private static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        var clean = url.Trim();
        if (clean.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            clean = clean["https://".Length..];
        else if (clean.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            clean = clean["http://".Length..];
        if (clean.Contains(' ')) return false;
        var dotIndex = clean.IndexOf('.');
        if (dotIndex <= 0 || dotIndex >= clean.Length - 1) return false;
        return true;
    }
}
