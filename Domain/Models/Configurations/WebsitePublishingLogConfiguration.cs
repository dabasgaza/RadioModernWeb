using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Models.Configurations;

public class WebsitePublishingLogConfiguration : IEntityTypeConfiguration<WebsitePublishingLog>
{
    public void Configure(EntityTypeBuilder<WebsitePublishingLog> builder)
    {
        builder.HasKey(e => e.WebsitePublishingLogId);

        builder.Property(e => e.Title)
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

        builder.HasOne(e => e.Episode)
               .WithMany(ep => ep.WebsitePublishingLogs)
               .HasForeignKey(e => e.EpisodeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PublishedByUser)
               .WithMany(u => u.WebsitePublishingLogs)
               .HasForeignKey(e => e.PublishedByUserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.EpisodeId)
               .HasDatabaseName("IX_WebsitePublishingLog_EpisodeId");

        builder.HasIndex(e => e.PublishedAt)
               .HasDatabaseName("IX_WebsitePublishingLog_PublishedAt");

        builder.HasIndex(e => e.PublishedByUserId)
               .HasDatabaseName("IX_WebsitePublishingLog_PublishedByUserId");
    }
}
