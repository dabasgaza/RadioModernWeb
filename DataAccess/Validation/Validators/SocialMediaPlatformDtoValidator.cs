// ============================================================
// SocialMediaPlatformDtoValidator — التحقق من المنصة
// ============================================================
// المسؤولية: تعريف التحقق من المنصة.
// ============================================================
using DataAccess.DTOs;
using FluentValidation;

namespace DataAccess.Validation.Validators;

/// <summary>
/// صنف التحقق من المنصة.
/// </summary>
public class SocialMediaPlatformDtoValidator : AbstractValidator<SocialMediaPlatformDto>
{
    public SocialMediaPlatformDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم المنصة مطلوب.");
    }
}
