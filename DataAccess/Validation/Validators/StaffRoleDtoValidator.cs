// ============================================================
// StaffRoleDtoValidator — التحقق من المسمى الوظيفي
// ============================================================
// المسؤولية: تعريف التحقق من المسمى الوظيفي.
// ============================================================
using DataAccess.DTOs;
using FluentValidation;

namespace DataAccess.Validation.Validators;

/// <summary>
/// صنف التحقق من المسمى الوظيفي.
/// </summary>
public class StaffRoleDtoValidator : AbstractValidator<StaffRoleDto>
{
    public StaffRoleDtoValidator()
    {
        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("اسم الدور الوظيفي مطلوب.");
    }
}
