// ============================================================
// CoverageDtoValidator — التحقق من التغطية
// ============================================================
// المسؤولية: تعريف التحقق من التغطية.
// ============================================================
using DataAccess.DTOs;
using FluentValidation;

namespace DataAccess.Validation.Validators;

/// <summary>
/// صنف التحقق من التغطية.
/// </summary>
public class CoverageDtoValidator : AbstractValidator<CoverageDto>
{
    public CoverageDtoValidator()
    {
        RuleFor(x => x.CorrespondentId)
            .GreaterThan(0).WithMessage("يرجى اختيار المراسل المسؤول.");

        RuleFor(x => x.Topic)
            .NotEmpty().WithMessage("يرجى إدخال موضوع التغطية.");
    }
}
