// ============================================================
// CorrespondentDtoValidator — التحقق من المراسل
// ============================================================
// المسؤولية: تعريف التحقق من المراسل.
// ============================================================
using DataAccess.DTOs;
using FluentValidation;

namespace DataAccess.Validation.Validators;

/// <summary>
/// صنف التحقق من المراسل.
/// </summary>
public class CorrespondentDtoValidator : AbstractValidator<CorrespondentDto>
{
    public CorrespondentDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("اسم المراسل مطلوب.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("رقم هاتف المراسل مطلوب.");
    }
}
