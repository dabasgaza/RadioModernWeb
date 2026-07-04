// ============================================================
// SocialMediaPlatformConfiguration — منصة التواصل
// ============================================================
// المسؤولية: تعريف منصة التواصل.
// ============================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Models.Configurations;

/// <summary>
/// صنف منصة التواصل.
/// </summary>
public class SocialMediaPlatformConfiguration : IEntityTypeConfiguration<SocialMediaPlatform>
{
    /// <summary>
    /// إعداد منصة التواصل.
    /// </summary>
    public void Configure(EntityTypeBuilder<SocialMediaPlatform> builder)
    {
        builder.HasKey(e => e.SocialMediaPlatformId);

        builder.Property(e => e.Name)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(e => e.Icon)
               .HasMaxLength(100);

        builder.Property(e => e.BaseUrl)
               .HasMaxLength(500);

        builder.Property(e => e.CreatedAt)
               .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.UpdatedAt)
               .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.IsActive)
               .HasDefaultValue(true);

        builder.Property(e => e.RowVersion)
               .IsRowVersion()
               .IsConcurrencyToken();
    }
}
