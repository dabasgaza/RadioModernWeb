// ============================================================
// SocialMediaPublishingLogConfiguration — سجل النشر الرقمي
// ============================================================
// المسؤولية: تعريف سجل النشر الرقمي.
// ============================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Models.Configurations;

/// <summary>
/// صنف سجل النشر الرقمي.
/// </summary>
public class SocialMediaPublishingLogConfiguration : IEntityTypeConfiguration<SocialMediaPublishingLog>
{
    /// <summary>
    /// إعداد سجل النشر الرقمي.
    /// </summary>
    public void Configure(EntityTypeBuilder<SocialMediaPublishingLog> builder)
    {
        builder.HasKey(e => e.SocialMediaPublishingLogId);

        builder.Property(e => e.ClipTitle)
               .HasMaxLength(500);

        builder.Property(e => e.Notes)
               .HasMaxLength(2000);

        builder.Property(e => e.PublishedAt)
               .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.CreatedAt)
               .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.UpdatedAt)
               .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.IsActive)
               .HasDefaultValue(true);

        builder.Property(e => e.RowVersion)
               .IsRowVersion()
               .IsConcurrencyToken();

        builder.HasOne(e => e.EpisodeGuest)
               .WithMany(g => g.SocialMediaPublishingLogs)
               .HasForeignKey(e => e.EpisodeGuestId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.PublishedByUser)
               .WithMany(u => u.SocialMediaPublishingLogs)
               .HasForeignKey(e => e.PublishedByUserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.EpisodeGuestId)
               .HasDatabaseName("IX_SocialMediaPublishingLog_EpisodeGuestId");

        builder.HasIndex(e => e.PublishedAt)
               .HasDatabaseName("IX_SocialMediaPublishingLog_PublishedAt");

        builder.HasIndex(e => e.PublishedByUserId)
               .HasDatabaseName("IX_SocialMediaPublishingLog_PublishedByUserId");
    }
}
