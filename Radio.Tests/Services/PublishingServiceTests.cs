using DataAccess.Common;
using DataAccess.DTOs;
using DataAccess.Services;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Radio.Tests.Helpers;
using Radio.Tests.TestData.Builders;
using Radio.Tests.TestData.Fixtures;

namespace Radio.Tests.Services;

[Collection("Sequential")]
public class PublishingServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _db;
    private readonly IPublishingService _service;
    private readonly UserSession _admin = UserSessionBuilder.CreateAdmin();

    public PublishingServiceTests(DatabaseFixture db)
    {
        _db = db;
        var sc = new ServiceCollection();
        sc.AddHybridCache();
        var cache = sc.BuildServiceProvider().GetRequiredService<HybridCache>(); // ponytail: real HybridCache beats mock boilerplate
        _service = new PublishingService(db.DbContextFactory, cache, TestTelemetry.Client);
    }

    [Fact]
    public async Task GetAllPlatformsAsync_ReturnsPlatforms()
    {
        var result = await _service.GetAllPlatformsAsync();
        result.Should().NotBeEmpty();
        result.Should().Contain(p => p.Name == "Facebook");
    }

    [Fact]
    public async Task LogSocialPublishingAsync_ValidLog_ReturnsSuccess()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.Episodes.Add(new Episode
        {
            EpisodeId = 10, ProgramId = 1, EpisodeName = "SocialPub", StatusId = 1,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        var guest = new Guest { FullName = "Guest1", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        ctx.Guests.Add(guest);
        await ctx.SaveChangesAsync();

        ctx.EpisodeGuests.Add(new EpisodeGuest
        {
            EpisodeId = 10, GuestId = guest.GuestId, Topic = "Politics",
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var eg = await ctx.EpisodeGuests.FirstAsync();
        var log = TestDataFactory.CreateSocialLog(episodeGuestId: eg.EpisodeGuestId, episodeId: 10);

        var result = await _service.LogSocialPublishingAsync(10, [log], _admin);

        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task LogSocialPublishingAsync_WithoutPermission_ReturnsFail()
    {
        var user = UserSessionBuilder.CreateLimited();
        var log = TestDataFactory.CreateSocialLog(episodeGuestId: 1, episodeId: 1);

        var result = await _service.LogSocialPublishingAsync(1, [log], user);

        result.ShouldBeFailure("صلاحية");
    }

    [Fact]
    public async Task LogWebsitePublishingAsync_Valid_ReturnsSuccess()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.Episodes.Add(new Episode
        {
            EpisodeId = 20, ProgramId = 1, EpisodeName = "WebTest", StatusId = 1,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var result = await _service.LogWebsitePublishingAsync(20, "Title", MediaType.Audio, "Notes", _admin);

        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task LogWebsitePublishingAsync_WithoutPermission_ReturnsFail()
    {
        var user = UserSessionBuilder.CreateLimited();

        var result = await _service.LogWebsitePublishingAsync(1, "Title", MediaType.Audio, "Notes", user);

        result.ShouldBeFailure("صلاحية");
    }

    [Fact]
    public async Task LogWebsitePublishingAsync_EpisodeNotFound_ReturnsFail()
    {
        var result = await _service.LogWebsitePublishingAsync(9999, "Title", MediaType.Audio, "Notes", _admin);

        result.ShouldBeFailure("لم يتم العثور");
    }

    [Fact]
    public async Task GetSocialPublishingLogAsync_Existing_ReturnsDto()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.Episodes.Add(new Episode
        {
            EpisodeId = 30, ProgramId = 1, EpisodeName = "SocialLog", StatusId = 1,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        var guest = new Guest { FullName = "G", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        ctx.Guests.Add(guest);
        await ctx.SaveChangesAsync();

        ctx.EpisodeGuests.Add(new EpisodeGuest
        {
            EpisodeGuestId = 50, EpisodeId = 30, GuestId = guest.GuestId,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        ctx.SocialMediaPublishingLogs.Add(new SocialMediaPublishingLog
        {
            EpisodeGuestId = 50, PublishedByUserId = 1, MediaType = MediaType.Audio,
            ClipTitle = "My Clip", ClipDuration = TimeSpan.FromMinutes(3),
            PublishedAt = DateTime.UtcNow, IsActive = true,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var result = await _service.GetSocialPublishingLogAsync(50);

        result.Should().NotBeNull();
        result!.ClipTitle.Should().Be("My Clip");
        result.EpisodeId.Should().Be(30);
    }

    [Fact]
    public async Task GetSocialPublishingLogByIdAsync_Existing_ReturnsDto()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.Episodes.Add(new Episode
        {
            EpisodeId = 40, ProgramId = 1, EpisodeName = "SocialById", StatusId = 1,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        var guest = new Guest { FullName = "G2", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        ctx.Guests.Add(guest);
        await ctx.SaveChangesAsync();

        ctx.EpisodeGuests.Add(new EpisodeGuest
        {
            EpisodeGuestId = 60, EpisodeId = 40, GuestId = guest.GuestId,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        ctx.SocialMediaPublishingLogs.Add(new SocialMediaPublishingLog
        {
            SocialMediaPublishingLogId = 70, EpisodeGuestId = 60,
            PublishedByUserId = 1, MediaType = MediaType.Video,
            ClipTitle = "Video Clip", PublishedAt = DateTime.UtcNow,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var result = await _service.GetSocialPublishingLogByIdAsync(70);

        result.Should().NotBeNull();
        result!.ClipTitle.Should().Be("Video Clip");
        result.EpisodeId.Should().Be(40);
    }

    [Fact]
    public async Task GetSocialPublishingLogByIdAsync_NotFound_ReturnsNull()
    {
        var result = await _service.GetSocialPublishingLogByIdAsync(9999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateSocialPublishingLogAsync_Valid_ReturnsSuccess()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.Episodes.Add(new Episode
        {
            EpisodeId = 50, ProgramId = 1, EpisodeName = "UpdateSocial", StatusId = 1,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        var guest = new Guest { FullName = "G3", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        ctx.Guests.Add(guest);
        await ctx.SaveChangesAsync();

        ctx.EpisodeGuests.Add(new EpisodeGuest
        {
            EpisodeGuestId = 80, EpisodeId = 50, GuestId = guest.GuestId,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        ctx.SocialMediaPublishingLogs.Add(new SocialMediaPublishingLog
        {
            SocialMediaPublishingLogId = 90, EpisodeGuestId = 80,
            PublishedByUserId = 1, MediaType = MediaType.Audio,
            PublishedAt = DateTime.UtcNow, IsActive = true,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var dto = new SocialMediaPublishingLogDto(
            LogId: 90, EpisodeGuestId: 80, EpisodeId: 50,
            ClipTitle: "Updated Title", Duration: TimeSpan.FromMinutes(5),
            MediaType: MediaType.Video,
            Platforms: [new PlatformPublishDto(1, "Facebook", "https://fb.com")]);

        var result = await _service.UpdateSocialPublishingLogAsync(dto, _admin);

        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task DeleteSocialPublishingLogAsync_Valid_SoftDeletes()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.Episodes.Add(new Episode
        {
            EpisodeId = 60, ProgramId = 1, EpisodeName = "DelSocial", StatusId = 1,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        var guest = new Guest { FullName = "G4", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        ctx.Guests.Add(guest);
        await ctx.SaveChangesAsync();

        ctx.EpisodeGuests.Add(new EpisodeGuest
        {
            EpisodeGuestId = 85, EpisodeId = 60, GuestId = guest.GuestId,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        ctx.SocialMediaPublishingLogs.Add(new SocialMediaPublishingLog
        {
            SocialMediaPublishingLogId = 95, EpisodeGuestId = 85,
            PublishedByUserId = 1, MediaType = MediaType.Audio,
            PublishedAt = DateTime.UtcNow, IsActive = true,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var result = await _service.DeleteSocialPublishingLogAsync(95, _admin);

        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task DeleteWebsitePublishingLogAsync_Valid_SoftDeletes()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.WebsitePublishingLogs.Add(new WebsitePublishingLog
        {
            WebsitePublishingLogId = 100, EpisodeId = 1, PublishedByUserId = 1,
            MediaType = MediaType.Audio, PublishedAt = DateTime.UtcNow,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var result = await _service.DeleteWebsitePublishingLogAsync(100, _admin);

        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task GetEpisodeSocialLogsAsync_ReturnsLogsForEpisode()
    {
        await using var ctx = await _db.CreateContextAsync();
        var guest = new Guest { FullName = "G5", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        ctx.Guests.Add(guest);
        await ctx.SaveChangesAsync();

        ctx.EpisodeGuests.Add(new EpisodeGuest
        {
            EpisodeGuestId = 200, EpisodeId = 42, GuestId = guest.GuestId,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        ctx.SocialMediaPublishingLogs.Add(new SocialMediaPublishingLog
        {
            EpisodeGuestId = 200, PublishedByUserId = 1, MediaType = MediaType.Audio,
            ClipTitle = "Ep42 Clip", PublishedAt = DateTime.UtcNow,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var result = await _service.GetEpisodeSocialLogsAsync(42);

        result.Should().HaveCount(1);
        result[0].ClipTitle.Should().Be("Ep42 Clip");
    }

    [Fact]
    public async Task GetAllPublishingRecordsAsync_ReturnsAll()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.Episodes.Add(new Episode
        {
            EpisodeId = 99, ProgramId = 1, EpisodeName = "PubRec", StatusId = 2,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });

        var guest = new Guest { FullName = "PubGuest", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        ctx.Guests.Add(guest);
        await ctx.SaveChangesAsync();

        ctx.EpisodeGuests.Add(new EpisodeGuest
        {
            EpisodeGuestId = 300, EpisodeId = 99, GuestId = guest.GuestId,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        ctx.SocialMediaPublishingLogs.Add(new SocialMediaPublishingLog
        {
            EpisodeGuestId = 300, PublishedByUserId = 1, MediaType = MediaType.Audio,
            PublishedAt = DateTime.UtcNow, IsActive = true,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var result = await _service.GetAllPublishingRecordsAsync();

        result.Should().NotBeEmpty();
    }
}
