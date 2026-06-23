using DataAccess.DTOs;
using FluentValidation;

namespace DataAccess.Validation.Validators;

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
