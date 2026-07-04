// ============================================================
// GuestDtoValidator — التحقق من الضيف
// ============================================================
// المسؤولية: تعريف التحقق من الضيف.
// ============================================================
using DataAccess.DTOs;
using FluentValidation;

namespace DataAccess.Validation.Validators;

/// <summary>
/// صنف التحقق من الضيف.
/// </summary>
public class GuestDtoValidator : AbstractValidator<GuestDto>
{
    public GuestDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("الاسم الكامل للضيف مطلوب.");

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.PhoneNumber) || !string.IsNullOrWhiteSpace(x.EmailAddress))
            .WithMessage("يجب إدخال رقم الهاتف أو البريد الإلكتروني على الأقل للتواصل.");
    }
}
