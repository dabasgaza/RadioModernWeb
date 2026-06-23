using DataAccess.DTOs;
using FluentValidation;

namespace DataAccess.Validation.Validators;

public class EpisodeDtoValidator : AbstractValidator<EpisodeDto>
{
    public EpisodeDtoValidator()
    {
        RuleFor(x => x.ProgramId)
            .GreaterThan(0).WithMessage("يرجى اختيار البرنامج من القائمة المنسدلة.");

        RuleFor(x => x.EpisodeName)
            .NotEmpty().WithMessage("عنوان الحلقة مطلوب ولا يمكن تركه فارغاً.");

        RuleFor(x => x.ScheduledDate)
            .NotNull().WithMessage("يرجى تحديد تاريخ تنفيذ الحلقة.");
    }
}
