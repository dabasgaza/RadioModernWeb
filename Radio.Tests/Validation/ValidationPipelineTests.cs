// ============================================================
// ValidationPipelineTests — التحقق من الصحة
// ============================================================
// المسؤولية: تعريف التحقق من الصحة.
// ============================================================
using DataAccess.Common;
using DataAccess.DTOs;
using DataAccess.Validation;
using Domain.Models;
using Radio.Tests.Helpers;

namespace Radio.Tests.Validation;

/// <summary>
/// صنف التحقق من الصحة.
/// </summary>
public class ValidationPipelineTests
{
    /// <summary>
    /// التحقق من صحة Publishing Log_ Valid_ Returns Success.
    /// </summary>
    [Fact]
    public void ValidatePublishingLog_Valid_ReturnsSuccess()
    {
        var dto = CreateLog(episodeGuestId: 1, title: "Valid Clip");

        var result = ValidationPipeline.ValidatePublishingLog(dto);

        result.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// التحقق من صحة Publishing Log_ Invalid الضيف Id_ Returns Error.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ValidatePublishingLog_InvalidGuestId_ReturnsError(int episodeGuestId)
    {
        var dto = CreateLog(episodeGuestId: episodeGuestId);

        var result = ValidationPipeline.ValidatePublishingLog(dto);

        result.ShouldBeFailure("ضيف");
    }

    /// <summary>
    /// التحقق من صحة Publishing Log_ Empty Clip Title_ Returns Error.
    /// </summary>
    [Fact]
    public void ValidatePublishingLog_EmptyClipTitle_ReturnsError()
    {
        var dto = CreateLog(title: "");

        var result = ValidationPipeline.ValidatePublishingLog(dto);

        result.ShouldBeFailure("عنوان");
    }

    /// <summary>
    /// التحقق من صحة Publishing Log_ Null Clip Title_ Returns Error.
    /// </summary>
    [Fact]
    public void ValidatePublishingLog_NullClipTitle_ReturnsError()
    {
        var dto = CreateLog(title: null!);

        var result = ValidationPipeline.ValidatePublishingLog(dto);

        result.ShouldBeFailure("عنوان");
    }

    /// <summary>
    /// التحقق من صحة Publishing Log_ No Platforms_ Returns Error.
    /// </summary>
    [Fact]
    public void ValidatePublishingLog_NoPlatforms_ReturnsError()
    {
        var dto = CreateLog(platformIds: []);

        var result = ValidationPipeline.ValidatePublishingLog(dto);

        result.ShouldBeFailure("منصة");
    }

    /// <summary>
    /// التحقق من صحة Publishing Log_ Invalid Url_ Returns Error.
    /// </summary>
    [Fact]
    public void ValidatePublishingLog_InvalidUrl_ReturnsError()
    {
        var dto = CreateLog(platformIds: [1], url: "not-a-valid-url");

        var result = ValidationPipeline.ValidatePublishingLog(dto);

        result.ShouldBeFailure("غير صالحة");
    }

    /// <summary>
    /// التحقق من صحة Publishing Log_ Missing Urls_ Returns Error.
    /// </summary>
    [Fact]
    public void ValidatePublishingLog_MissingUrls_ReturnsError()
    {
        var dto = CreateLog(platformIds: [1], url: "");

        var result = ValidationPipeline.ValidatePublishingLog(dto);

        result.ShouldBeFailure("رابط");
    }

    /// <summary>
    /// التحقق من صحة Publishing Batch_ Empty List_ Returns Error.
    /// </summary>
    [Fact]
    public void ValidatePublishingBatch_EmptyList_ReturnsError()
    {
        var result = ValidationPipeline.ValidatePublishingBatch([]);

        result.ShouldBeFailure("بيانات");
    }

    /// <summary>
    /// التحقق من صحة Publishing Batch_ Null List_ Returns Error.
    /// </summary>
    [Fact]
    public void ValidatePublishingBatch_NullList_ReturnsError()
    {
        var result = ValidationPipeline.ValidatePublishingBatch(null!);

        result.ShouldBeFailure("بيانات");
    }

    /// <summary>
    /// التحقق من صحة Publishing Batch_ Valid List_ Returns Success.
    /// </summary>
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
