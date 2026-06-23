using DataAccess.DTOs;
using FluentValidation;

namespace DataAccess.Validation.Validators;

public class SocialMediaPlatformDtoValidator : AbstractValidator<SocialMediaPlatformDto>
{
    public SocialMediaPlatformDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم المنصة مطلوب.");
    }
}
