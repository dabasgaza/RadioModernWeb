// ============================================================
// BroadcastWorkflowDBContextFactory — مصنع السياق
// ============================================================
// المسؤولية: تعريف مصنع السياق.
// ============================================================
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Domain;

/// <summary>
/// مصنع وقت التصميم لـ EF Core migrations — يُستخدم فقط بواسطة `dotnet ef`.
/// في وقت التشغيل، يُسجَّل الـ DbContextFactory في Radio.Web/Program.cs.
/// <summary>
/// صنف مصنع السياق.
/// </summary>
/// </summary>
public class BroadcastWorkflowDBContextFactory : IDesignTimeDbContextFactory<BroadcastWorkflowDBContext>
{
    /// <summary>
    /// إنشاء Db السياق.
    /// </summary>
    public BroadcastWorkflowDBContext CreateDbContext(string[] args)
    {
        // ابحث عن appsettings.json في Radio.Web (المشروع الذي يحتوي على ملفات الإعداد)
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../Radio.Web");
        if (!Directory.Exists(basePath))
        {
            // fallback: استخدم المسار الحالي
            basePath = Directory.GetCurrentDirectory();
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // fallback لمتغير البيئة إن لم يُضبط في appsettings
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = Environment.GetEnvironmentVariable("RADIO_CONNECTION_STRING");
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "نص الاتصال غير موجود. تأكد من إعداد ConnectionStrings:DefaultConnection في Radio.Web/appsettings.json " +
                "أو ضبط متغير البيئة RADIO_CONNECTION_STRING.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<BroadcastWorkflowDBContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new BroadcastWorkflowDBContext(optionsBuilder.Options);
    }
}
