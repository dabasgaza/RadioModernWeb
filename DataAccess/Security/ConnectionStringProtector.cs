using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;

namespace DataAccess.Security;

/// <summary>
/// تشفير وفك تشفير نص الاتصال بقاعدة البيانات باستخدام ASP.NET Core Data Protection API.
///
/// ✨ هذا التنفيذ بديل حديث ومتعدد المنصات (Cross-Platform) للـ DPAPI الخاص بـ Windows.
/// يعمل على Windows و Linux و macOS — مثالي لبيئات الحاويات (Docker) والسحابة.
///
/// 📋 الاستخدام:
///   1. في وضع التطوير: نص الاتصال يبقى بصيغة نص عادي في appsettings.json
///   2. في وضع الإنتاج: شفّر النص باستخدام ConnectionStringProtector.Encrypt()
///      ثم ضعه في appsettings.json مع بادئة "ENC:" مثل:
///      "DefaultConnection": "ENC:AQAAANCMnd8BFdERjH..."
///   3. يمكن أيضاً استخدام متغير البيئة RADIO_CONNECTION_STRING كأولوية قصوى
///
/// 🔐 ملاحظات الأمان:
///   - مفاتيح التشفير تُدار بواسطة IDataProtectionProvider (افتراضياً في ~/.aspnet/DataProtection-Keys)
///   - في النشر الإنتاجي، يجب توجيه مفاتيح التشفير إلى موقع مشترك (KeyVault, FileShare, Redis)
///   - لا يمكن فك التشفير دون الوصول لنفس المفاتيح
/// </summary>
public class ConnectionStringProtector
{
    private readonly IDataProtector _protector;

    /// <summary>
    /// بادئة تحدد أن القيمة مشفرة.
    /// </summary>
    public const string EncryptedPrefix = "ENC:";

    public ConnectionStringProtector(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector("Radio.BroadcastWorkflow.ConnectionString.v1");
    }

    /// <summary>
    /// تشفير نص الاتصال.
    /// </summary>
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            throw new ArgumentException("نص الاتصال لا يمكن أن يكون فارغاً.", nameof(plainText));

        var plainBytes = Encoding.Unicode.GetBytes(plainText);
        var encryptedBytes = _protector.Protect(plainBytes);
        return Convert.ToBase64String(encryptedBytes);
    }

    /// <summary>
    /// فك تشفير نص الاتصال.
    /// </summary>
    public string Decrypt(string encryptedBase64)
    {
        if (string.IsNullOrWhiteSpace(encryptedBase64))
            throw new ArgumentException("النص المشفر لا يمكن أن يكون فارغاً.", nameof(encryptedBase64));

        var encryptedBytes = Convert.FromBase64String(encryptedBase64);
        var plainBytes = _protector.Unprotect(encryptedBytes);
        return Encoding.Unicode.GetString(plainBytes);
    }

    /// <summary>
    /// فحص ما إذا كانت القيمة مشفرة (تبدأ بـ "ENC:").
    /// </summary>
    public static bool IsEncrypted(string? value)
    {
        return !string.IsNullOrEmpty(value) && value.StartsWith(EncryptedPrefix);
    }

    /// <summary>
    /// فك تشفير القيمة تلقائياً إذا كانت مشفرة، أو إرجاعها كما هي.
    /// </summary>
    public string UnprotectIfEncrypted(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value!;

        if (value.StartsWith(EncryptedPrefix))
        {
            var encryptedPart = value[EncryptedPrefix.Length..];
            return Decrypt(encryptedPart);
        }

        return value;
    }

    /// <summary>
    /// محاولة فك التشفير بأمان — ترجع false بدلاً من رمي استثناء عند الفشل.
    /// </summary>
    public bool TryUnprotect(string? value, out string result)
    {
        try
        {
            result = UnprotectIfEncrypted(value);
            return true;
        }
        catch
        {
            result = string.Empty;
            return false;
        }
    }
}
