// ============================================================
// GuestConfiguration — الضيف
// ============================================================
// المسؤولية: تعريف الضيف.
// ============================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Models.Configurations;

/// <summary>
/// صنف الضيف.
/// </summary>
public class GuestConfiguration : IEntityTypeConfiguration<Guest>
{
    /// <summary>
    /// إعداد الضيف.
    /// </summary>
    public void Configure(EntityTypeBuilder<Guest> builder)
    {
        // 1. المفتاح الأساسي (إزالة الاسم القبيح المولد آلياً)
        builder.HasKey(e => e.GuestId);

        // 2. إعدادات الخصائص (Properties)
        builder.Property(e => e.FullName)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(e => e.PhoneNumber)
               .HasMaxLength(20);

        builder.Property(e => e.EmailAddress)
               .HasMaxLength(255);

        builder.Property(e => e.Organization)
               .HasMaxLength(200);

        // إعدادات BaseEntity
        builder.Property(e => e.CreatedAt)
               .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.UpdatedAt)
               .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.IsActive)
               .HasDefaultValue(true);

        builder.Property(e => e.RowVersion)
               .IsRowVersion()
               .IsConcurrencyToken();

        // 3. العلاقات (Relationships)



        // ملاحظة: علاقة الضيف مع الحلقات (EpisodeGuests) و التغطيات (CorrespondentCoverages) 
        // يتم تعريفها من الجانب الآخر (في EpisodeGuestConfiguration و CorrespondentCoverageConfiguration)
        // كقاعدة أفضل ممارسة في EF Core: قم بتعريف العلاقة في كيان واحد فقط لتجنب التكرار والتعارض.

        // 4. الفهارس (Indexes)
        // فهرس مصفى لتسريع البحث بالاسم للضيوف النشطين
        builder.HasIndex(e => e.FullName)
               .HasDatabaseName("IX_Guests_Active_FullName")
               .HasFilter("[IsActive] = 1");

        // 5. Seed Data — handled by DbSeeder at runtime

    }
}