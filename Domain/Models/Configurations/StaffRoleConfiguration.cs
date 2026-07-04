// ============================================================
// StaffRoleConfiguration — المسمى الوظيفي
// ============================================================
// المسؤولية: تعريف المسمى الوظيفي.
// ============================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Models.Configurations;

/// <summary>
/// صنف المسمى الوظيفي.
/// </summary>
public class StaffRoleConfiguration : IEntityTypeConfiguration<StaffRole>
{
    /// <summary>
    /// إعداد المسمى الوظيفي.
    /// </summary>
    public void Configure(EntityTypeBuilder<StaffRole> builder)
    {
        builder.HasKey(e => e.StaffRoleId);

        builder.Property(e => e.RoleName)
               .IsRequired()
               .HasMaxLength(100);

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
