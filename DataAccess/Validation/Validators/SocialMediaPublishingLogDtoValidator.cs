// ============================================================
// SocialMediaPublishingLogDtoValidator — التحقق من سجل النشر
// ============================================================
// المسؤولية: تعريف التحقق من سجل النشر.
// ============================================================
using DataAccess.DTOs;
using FluentValidation;

namespace DataAccess.Validation.Validators;

/// <summary>
/// صنف التحقق من سجل النشر.
/// </summary>
public class SocialMediaPublishingLogDtoValidator : AbstractValidator<SocialMediaPublishingLogDto>
{
    public SocialMediaPublishingLogDtoValidator()
    {
        RuleFor(x => x.EpisodeGuestId)
            .GreaterThan(0).WithMessage("يرجى اختيار ضيف لربط سجل النشر به.");

        RuleFor(x => x.ClipTitle)
            .NotEmpty().WithMessage("عنوان المقطع مطلوب ولا يمكن تركه فارغاً.");

        RuleFor(x => x.Platforms)
            .NotNull().WithMessage("يجب اختيار منصة نشر واحدة على الأقل مع إدخال الرابط.")
            .Must(x => x != null && x.Any(p => !string.IsNullOrWhiteSpace(p.Url)))
            .WithMessage("يجب اختيار منصة نشر واحدة على الأقل مع إدخال الرابط.");

        RuleForEach(x => x.Platforms)
            .SetValidator(new PlatformPublishDtoValidator());

        RuleFor(x => x.Duration)
            .Must(d => d is null || d.Value.TotalSeconds > 0)
            .WithMessage("المدة يجب أن تكون أكبر من صفر.")
            .Must(d => d is null || d.Value.TotalHours <= 12)
            .WithMessage("المدة لا يمكن أن تتجاوز 12 ساعة.");
    }
}
