using Microsoft.Extensions.Configuration;
using Serilog;

namespace DataAccess.Security;

/// <summary>
/// مزود التهيئة الآمن — يقرأ نص الاتصال من عدة مصادر بترتيب أولوية:
///
/// 🥇 الأولوية 1: متغير البيئة RADIO_CONNECTION_STRING
///     - الأمان الأعلى — لا يُخزّن في ملفات المشروع
///     - مفضل في بيئات الإنتاج والخوادم والحاويات
///     - يُعين عبر: export RADIO_CONNECTION_STRING="Server=.;Database=..."
///
/// 🥈 الأولوية 2: نص الاتصال المشفر في appsettings.json (بادئة ENC:)
///     - أمان متوسط — مشفر بـ ASP.NET Core Data Protection
///     - مفضل في بيئات الإنتاج
///
/// 🥉 الأولوية 3: نص الاتصال بصيغة نص عادي في appsettings.json
///     - بدون أمان — مخصص لبيئة التطوير فقط
/// </summary>
public class SecureConfigurationProvider
{
    private readonly ConnectionStringProtector _protector;

    public SecureConfigurationProvider(ConnectionStringProtector protector)
    {
        _protector = protector;
    }

    /// <summary>
    /// اسم متغير البيئة لنص الاتصال (الأولوية القصوى).
    /// </summary>
    public const string EnvironmentVariableName = "RADIO_CONNECTION_STRING";

    /// <summary>
    /// قراءة نص الاتصال الآمن من التهيئة مع دعم التشفير ومتغيرات البيئة.
    /// </summary>
    public string GetSecureConnectionString(IConfiguration configuration, string connectionName = "DefaultConnection")
    {
        // 🥇 الأولوية 1: متغير البيئة
        var envValue = Environment.GetEnvironmentVariable(EnvironmentVariableName);
        if (!string.IsNullOrWhiteSpace(envValue))
        {
            Log.Information("نص الاتصال مقروء من متغير البيئة {EnvVar}", EnvironmentVariableName);
            return envValue;
        }

        // 🥈 الأولوية 2 و 3: من appsettings.json (مشفر أو نص عادي)
        var configValue = configuration.GetConnectionString(connectionName);
        if (string.IsNullOrWhiteSpace(configValue))
        {
            Log.Warning("نص الاتصال '{ConnName}' غير موجود في التهيئة أو متغير البيئة", connectionName);
            return string.Empty;
        }

        if (ConnectionStringProtector.IsEncrypted(configValue))
        {
            try
            {
                var decrypted = _protector.UnprotectIfEncrypted(configValue);
                Log.Information("نص الاتصال مشفر — تم فك التشفير بنجاح");
                return decrypted;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "فشل فك تشفير نص الاتصال — قد يكون التشفير تم بمفاتيح مختلفة");
                throw new InvalidOperationException(
                    "فشل فك تشفير نص الاتصال. تأكد من أن مفاتيح Data Protection متوفرة، " +
                    "أو استخدم متغير البيئة RADIO_CONNECTION_STRING كبديل.", ex);
            }
        }

        // نص عادي (بيئة التطوير)
        Log.Debug("نص الاتصال مقروء بصيغة نص عادي من appsettings.json — يُنصح بالتشفير للإنتاج");
        return configValue;
    }

    /// <summary>
    /// فحص حالة أمان نص الاتصال الحالي.
    /// </summary>
    public ConnectionStringSecurityStatus GetSecurityStatus(IConfiguration configuration, string connectionName = "DefaultConnection")
    {
        var envValue = Environment.GetEnvironmentVariable(EnvironmentVariableName);
        if (!string.IsNullOrWhiteSpace(envValue))
        {
            return new ConnectionStringSecurityStatus
            {
                Source = "متغير البيئة",
                IsEncrypted = true,
                IsSecure = true,
                Description = $"نص الاتصال مقروء من متغير البيئة {EnvironmentVariableName}"
            };
        }

        var configValue = configuration.GetConnectionString(connectionName);
        if (string.IsNullOrWhiteSpace(configValue))
        {
            return new ConnectionStringSecurityStatus
            {
                Source = "غير موجود",
                IsEncrypted = false,
                IsSecure = false,
                Description = "نص الاتصال غير موجود في التهيئة أو متغير البيئة"
            };
        }

        if (ConnectionStringProtector.IsEncrypted(configValue))
        {
            return new ConnectionStringSecurityStatus
            {
                Source = "appsettings.json (مشفر)",
                IsEncrypted = true,
                IsSecure = true,
                Description = "نص الاتصال مشفر بـ ASP.NET Core Data Protection"
            };
        }

        return new ConnectionStringSecurityStatus
        {
            Source = "appsettings.json (نص عادي)",
            IsEncrypted = false,
            IsSecure = false,
            Description = "⚠️ نص الاتصال بصيغة نص عادي — يُنصح بالتشفير للإنتاج"
        };
    }
}

/// <summary>
/// حالة أمان نص الاتصال — تُستخدم في شاشات التشخيص.
/// </summary>
public class ConnectionStringSecurityStatus
{
    public string Source { get; set; } = string.Empty;
    public bool IsEncrypted { get; set; }
    public bool IsSecure { get; set; }
    public string Description { get; set; } = string.Empty;
}
