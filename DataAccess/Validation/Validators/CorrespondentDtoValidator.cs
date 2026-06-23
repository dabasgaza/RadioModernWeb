using DataAccess.DTOs;
using FluentValidation;

namespace DataAccess.Validation.Validators;

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
