using DataAccess.DTOs;
using FluentValidation;

namespace DataAccess.Validation.Validators;

public class UserDtoValidator : AbstractValidator<UserDto>
{
    public UserDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("اسم المستخدم مطلوب.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("اسم الدخول مطلوب.");

        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("يرجى اختيار دور للمستخدم.");
    }
}

public class UserCreateValidator : AbstractValidator<(UserDto Dto, string? Password)>
{
    public UserCreateValidator()
    {
        RuleFor(x => x.Dto.FullName)
            .NotEmpty().WithMessage("اسم المستخدم مطلوب.");

        RuleFor(x => x.Dto.Username)
            .NotEmpty().WithMessage("اسم الدخول مطلوب.");

        RuleFor(x => x.Dto.RoleId)
            .GreaterThan(0).WithMessage("يرجى اختيار دور للمستخدم.");

        RuleFor(x => x.Password)
            .NotEmpty().When(x => x.Dto.UserId == 0)
            .WithMessage("كلمة المرور مطلوبة للمستخدم الجديد.");

        RuleFor(x => x.Password)
            .MinimumLength(6).When(x => !string.IsNullOrWhiteSpace(x.Password))
            .WithMessage("كلمة المرور يجب أن تكون 6 أحرف على الأقل.");
    }
}
