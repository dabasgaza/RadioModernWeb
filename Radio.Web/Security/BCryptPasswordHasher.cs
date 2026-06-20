using Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace Radio.Web.Security;

/// <summary>
/// PasswordHasher مخصص يستخدم BCrypt بدلاً من PBKDF2 الافتراضي.
///
/// السبب: النظام الأصلي يخزّن كلمات المرور بصيغة BCrypt ($2a$11$...).
/// Identity افتراضياً يستخدم PBKDF2، وهي غير متوافقة مع الـ hashes الموجودة.
///
/// هذا الـ Hasher:
///   - HashPassword: يستخدم BCrypt.HashPassword
///   - VerifyHashedPassword: يستخدم BCrypt.Verify ويرجع Success/Failure
/// </summary>
public class BCryptPasswordHasher : IPasswordHasher<ApplicationUser>
{
    private const int WorkFactor = 11; // نفس WorkFactor المستخدم في النظام الأصلي

    public string HashPassword(ApplicationUser user, string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
    }

    public PasswordVerificationResult VerifyHashedPassword(
        ApplicationUser user,
        string hashedPassword,
        string providedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword) || string.IsNullOrEmpty(providedPassword))
            return PasswordVerificationResult.Failed;

        // التحقق من أن الـ hash بصيغة BCrypt صحيحة
        if (!hashedPassword.StartsWith("$2"))
            return PasswordVerificationResult.Failed;

        try
        {
            var isValid = BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
            if (!isValid)
                return PasswordVerificationResult.Failed;

            return PasswordVerificationResult.Success;
        }
        catch
        {
            return PasswordVerificationResult.Failed;
        }
    }
}
