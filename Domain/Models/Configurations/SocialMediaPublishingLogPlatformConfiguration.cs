using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Models.Configurations;

public class SocialMediaPublishingLogPlatformConfiguration : IEntityTypeConfiguration<SocialMediaPublishingLogPlatform>
{
    public void Configure(EntityTypeBuilder<SocialMediaPublishingLogPlatform> builder)
    {
        builder.HasKey(e => e.SocialMediaPublishingLogPlatformId);

        builder.Property(e => e.Url)
               .HasMaxLength(1000);

        builder.Property(e => e.CreatedAt)
               .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.UpdatedAt)
               .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.IsActive)
               .HasDefaultValue(true);

        builder.Property(e => e.RowVersion)
               .IsRowVersion()
               .IsConcurrencyToken();

        builder.HasOne(e => e.SocialMediaPublishingLog)
               .WithMany(l => l.Platforms)
               .HasForeignKey(e => e.SocialMediaPublishingLogId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.SocialMediaPlatform)
               .WithMany(p => p.PublishingLogPlatforms)
               .HasForeignKey(e => e.SocialMediaPlatformId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.SocialMediaPublishingLogId)
               .HasDatabaseName("IX_LogPlatform_LogId");

        builder.HasIndex(e => e.SocialMediaPlatformId)
               .HasDatabaseName("IX_LogPlatform_PlatformId");
    }
}
