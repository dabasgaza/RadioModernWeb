using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Models.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(e => e.EmployeeId);

        builder.Property(e => e.FullName)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(e => e.Notes)
               .HasMaxLength(2000);

        builder.Property(e => e.CreatedAt)
               .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.UpdatedAt)
               .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.IsActive)
               .HasDefaultValue(true);

        builder.Property(e => e.RowVersion)
               .IsRowVersion()
               .IsConcurrencyToken();

        builder.HasOne(e => e.StaffRole)
               .WithMany(r => r.Employees)
               .HasForeignKey(e => e.StaffRoleId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.FullName)
               .HasDatabaseName("IX_Employees_Active_FullName")
               .HasFilter("[IsActive] = 1");
    }
}
