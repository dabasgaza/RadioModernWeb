using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Models.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(e => e.RoleId);

        builder.Property(e => e.CreatedAt)
               .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.UpdatedAt)
               .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.RowVersion)
               .IsRowVersion()
               .IsConcurrencyToken();

        builder.Property(e => e.RoleName)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(e => e.RoleDescription)
               .HasMaxLength(500);

        builder.HasIndex(e => e.RoleName)
              .IsUnique()
              .HasDatabaseName("UQ_Roles_RoleName");

        builder.HasData(
            new Role { RoleId = 1, RoleName = "Admin", RoleDescription = "مسؤول النظام — صلاحيات كاملة", IsActive = true, CreatedAt = new DateTime(2026, 4, 28, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 4, 28, 0, 0, 0, DateTimeKind.Utc) },
            new Role { RoleId = 2, RoleName = "ProgramMgr", RoleDescription = "مدير البرامج", IsActive = true, CreatedAt = new DateTime(2026, 4, 28, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 4, 28, 0, 0, 0, DateTimeKind.Utc) },
            new Role { RoleId = 3, RoleName = "Director", RoleDescription = "مخرج البث", IsActive = true, CreatedAt = new DateTime(2026, 4, 28, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 4, 28, 0, 0, 0, DateTimeKind.Utc) },
            new Role { RoleId = 4, RoleName = "WebPublisher", RoleDescription = "ناشر الموقع", IsActive = true, CreatedAt = new DateTime(2026, 4, 28, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 4, 28, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}