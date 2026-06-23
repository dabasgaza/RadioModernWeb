using DataAccess.DTOs;
using FluentValidation;

namespace DataAccess.Validation.Validators;

public class ProgramDtoValidator : AbstractValidator<ProgramDto>
{
    public ProgramDtoValidator()
    {
        RuleFor(x => x.ProgramName)
            .NotEmpty().WithMessage("اسم البرنامج مطلوب.");
    }
}
