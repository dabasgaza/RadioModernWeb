using DataAccess.DTOs;
using FluentValidation;

namespace DataAccess.Validation.Validators;

public class EmployeeDtoValidator : AbstractValidator<EmployeeDto>
{
    public EmployeeDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("اسم الموظف مطلوب.");

        RuleFor(x => x.StaffRoleId)
            .NotNull().WithMessage("يرجى اختيار دور وظيفي للموظف.")
            .GreaterThan(0).WithMessage("يرجى اختيار دور وظيفي للموظف.");
    }
}
