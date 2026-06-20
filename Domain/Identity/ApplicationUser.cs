using Microsoft.AspNetCore.Identity;

namespace Domain.Identity;

/// <summary>
/// يمثّل مستخدم التطبيق في نظام ASP.NET Core Identity.
/// يستخدم PK من نوع int (وليس GUID الافتراضي) ليتوافق مع جدول Users الأصلي في Domain.
///
/// العلاقة مع النظام الأصلي:
///   - IdentityUser (ApplicationUser): مسؤول عن المصادقة (Login, Password, Lockout, TwoFactor)
///   - Domain.User: مسؤول عن الـ Business Logic والـ Audit Trail (FKs في جميع الجداول)
///   - الربط: ApplicationUser.DomainUserId ↔ User.UserId
///
/// ملاحظة: UserName في Identity يطابق Username في جدول Users الأصلي.
/// </summary>
public class ApplicationUser : IdentityUser<int>
{
    /// <summary>الاسم الكامل للمستخدم (مطابق لـ User.FullName في Domain)</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>رقم الهاتف المعروض في الواجهة (مطابق لـ User.PhoneNumber)</summary>
    public string? DisplayPhoneNumber { get; set; }

    /// <summary>معرّف الـ User الأصلي في Domain (FK إلى Users.UserId)</summary>
    public int DomainUserId { get; set; }

    /// <summary>معرّف الدور الأصلي في Domain (FK إلى Roles.RoleId)</summary>
    public int DomainRoleId { get; set; }

    /// <summary>وقت آخر تسجيل دخول — يُحدَّث عند كل Login</summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>هل الحساب نشط في النظام القديم؟ (للتوافق مع Soft Delete)</summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// يمثّل دور التطبيق في نظام ASP.NET Core Identity.
/// يستخدم PK من نوع int ليتوافق مع جدول Roles الأصلي.
/// </summary>
public class ApplicationRole : IdentityRole<int>
{
    /// <summary>الوصف العربي للدور (مطابق لـ Role.RoleDescription في Domain)</summary>
    public string? RoleDescription { get; set; }

    /// <summary>معرّف الـ Role الأصلي في Domain</summary>
    public int DomainRoleId { get; set; }

    /// <summary>هل الدور نشط؟</summary>
    public bool IsActive { get; set; } = true;
}
