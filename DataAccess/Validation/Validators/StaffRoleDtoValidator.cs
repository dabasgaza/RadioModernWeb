using DataAccess.DTOs;
using FluentValidation;

namespace DataAccess.Validation.Validators;

public class StaffRoleDtoValidator : AbstractValidator<StaffRoleDto>
{
    public StaffRoleDtoValidator()
    {
        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("اسم الدور الوظيفي مطلوب.");
    }
}
