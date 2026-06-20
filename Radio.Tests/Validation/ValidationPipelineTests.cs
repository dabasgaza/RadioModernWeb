using DataAccess.Common;
using DataAccess.DTOs;
using DataAccess.Validation;
using Domain.Models;
using Radio.Tests.Helpers;

namespace Radio.Tests.Validation;

public class ValidationPipelineTests
{
    [Fact]
    public void ValidatePublishingLog_Valid_ReturnsSuccess()
    {
        var dto = CreateLog(episodeGuestId: 1, title: "Valid Clip");

        var result = ValidationPipeline.ValidatePublishingLog(dto);

        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ValidatePublishingLog_InvalidGuestId_ReturnsError(int episodeGuestId)
    {
        var dto = CreateLog(episodeGuestId: episodeGuestId);

        var result = ValidationPipeline.ValidatePublishingLog(dto);

        result.ShouldBeFailure("ضيف");
    }

    [Fact]
    public void ValidatePublishingLog_EmptyClipTitle_ReturnsError()
    {
        var dto = CreateLog(title: "");

        var result = ValidationPipeline.ValidatePublishingLog(dto);

        result.ShouldBeFailure("عنوان");
    }

    [Fact]
    public void ValidatePublishingLog_NullClipTitle_ReturnsError()
    {
        var dto = CreateLog(title: null!);

        var result = ValidationPipeline.ValidatePublishingLog(dto);

        result.ShouldBeFailure("عنوان");
    }

    [Fact]
    public void ValidatePublishingLog_NoPlatforms_ReturnsError()
    {
        var dto = CreateLog(platformIds: []);

        var result = ValidationPipeline.ValidatePublishingLog(dto);

        result.ShouldBeFailure("منصة");
    }

    [Fact]
    public void ValidatePublishingLog_InvalidUrl_ReturnsError()
    {
        var dto = CreateLog(platformIds: [1], url: "not-a-valid-url");

        var result = ValidationPipeline.ValidatePublishingLog(dto);

        result.ShouldBeFailure("غير صالحة");
    }

    [Fact]
    public void ValidatePublishingLog_MissingUrls_ReturnsError()
    {
        var dto = CreateLog(platformIds: [1], url: "");

        var result = ValidationPipeline.ValidatePublishingLog(dto);

        result.ShouldBeFailure("رابط");
    }

    [Fact]
    public void ValidatePublishingBatch_EmptyList_ReturnsError()
    {
        var result = ValidationPipeline.ValidatePublishingBatch([]);

        result.ShouldBeFailure("بيانات");
    }

    [Fact]
    public void ValidatePublishingBatch_NullList_ReturnsError()
    {
        var result = ValidationPipeline.ValidatePublishingBatch(null!);

        result.ShouldBeFailure("بيانات");
    }

    [Fact]
    public void ValidatePublishingBatch_ValidList_ReturnsSuccess()
    {
        var logs = new List<SocialMediaPublishingLogDto> { CreateLog() };

        var result = ValidationPipeline.ValidatePublishingBatch(logs);

        result.IsSuccess.Should().BeTrue();
    }

    private static SocialMediaPublishingLogDto CreateLog(
        int episodeGuestId = 1, int episodeId = 1,
        string title = "Clip", MediaType mediaType = MediaType.Audio,
        int[]? platformIds = null, string url = "https://example.com")
    {
        platformIds ??= [1];
        return new SocialMediaPublishingLogDto(
            LogId: 0, EpisodeGuestId: episodeGuestId, EpisodeId: episodeId,
            ClipTitle: title, Duration: TimeSpan.FromMinutes(5),
            MediaType: mediaType,
            Platforms: platformIds.Select(id =>
                new PlatformPublishDto(id, "Platform", url)).ToList());
    }
}
