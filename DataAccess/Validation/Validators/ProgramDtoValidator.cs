// ============================================================
// ProgramDtoValidator — التحقق من البرنامج
// ============================================================
// المسؤولية: تعريف التحقق من البرنامج.
// ============================================================
using DataAccess.DTOs;
using FluentValidation;

namespace DataAccess.Validation.Validators;

/// <summary>
/// صنف التحقق من البرنامج.
/// </summary>
public class ProgramDtoValidator : AbstractValidator<ProgramDto>
{
    public ProgramDtoValidator()
    {
        RuleFor(x => x.ProgramName)
            .NotEmpty().WithMessage("اسم البرنامج مطلوب.");
    }
}
