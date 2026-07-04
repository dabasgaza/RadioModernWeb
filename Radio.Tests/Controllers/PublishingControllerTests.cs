// ============================================================
// PublishingControllerTests — اختبارات النشر
// ============================================================
// المسؤولية: تعريف اختبارات النشر.
// ============================================================
using DataAccess.Common;
using DataAccess.DTOs;
using DataAccess.Services;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Radio.Tests.Helpers;
using Radio.Tests.TestData.Builders;
using Radio.Web.Controllers;
using Radio.Web.Services;
using Radio.Web.ViewModels;
using Moq;
using System.Threading;

namespace Radio.Tests.Controllers;

/// <summary>
/// صنف اختبارات النشر.
/// </summary>
public class PublishingControllerTests
{
    private readonly Mock<IPublishingQueryService> _query = new();
    private readonly Mock<IPublishingCommandService> _command = new();
    private readonly Mock<IEpisodeQueryService> _episodes = new();
    private readonly Mock<ICurrentUserService> _currentUser;
    private readonly Mock<ILogger<PublishingController>> _logger = new();
    private readonly PublishingController _controller;
    private readonly UserSession _admin = UserSessionBuilder.CreateAdmin();

    public PublishingControllerTests()
    {
        _currentUser = UserSessionBuilder.CreateMock(_admin);
        _controller = new PublishingController(
            _query.Object, _command.Object, _episodes.Object,
            _currentUser.Object, _logger.Object);

        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }

    /// <summary>
    /// Record.
    /// </summary>
    private static PublishingRecordDto Record(int id, string type, int epId, string epName, string progName, string summary, DateTime date, string by) =>
        new()
        {
            RecordId = id, RecordType = type, EpisodeId = epId,
            EpisodeName = epName, ProgramName = progName,
            Summary = summary, RecordDate = date, RecordedBy = by
        };

    /// <summary>
    /// عرض قائمة _ Returns View With Records.
    /// </summary>
    [Fact]
    public async Task Index_ReturnsViewWithRecords()
    {
        _query.Setup(q => q.GetAllPublishingRecordsAsync(null, CancellationToken.None))
            .ReturnsAsync([
                Record(1, "SocialMedia", 1, "Ep1", "P1", "summary", DateTime.UtcNow, "Admin"),
                Record(2, "Website", 2, "Ep2", "P1", "summary2", DateTime.UtcNow, "Admin")
            ]);

        var result = await _controller.Index(null);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeAssignableTo<IEnumerable<PublishingRecordDto>>();
    }

    /// <summary>
    /// عرض قائمة _ البحث Filter_ Returns Filtered.
    /// </summary>
    [Fact]
    public async Task Index_SearchFilter_ReturnsFiltered()
    {
        _query.Setup(q => q.GetAllPublishingRecordsAsync(null, CancellationToken.None))
            .ReturnsAsync([
                Record(1, "SocialMedia", 1, "Special Ep", "P1", "summary", DateTime.UtcNow, "Admin"),
                Record(2, "Website", 2, "Other Ep", "P1", "summary2", DateTime.UtcNow, "Admin")
            ]);

        var result = await _controller.Index("Special");

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model as List<PublishingRecordDto>;
        model.Should().HaveCount(1);
        model![0].EpisodeName.Should().Be("Special Ep");
    }

    /// <summary>
    /// تسجيل Social_ Get_ Existing Episode_ Returns View.
    /// </summary>
    [Fact]
    public async Task LogSocial_Get_ExistingEpisode_ReturnsView()
    {
        _episodes.Setup(e => e.GetActiveEpisodeByIdAsync(1, CancellationToken.None))
            .ReturnsAsync(new ActiveEpisodeDto { EpisodeId = 1, EpisodeName = "Ep1" });
        _episodes.Setup(e => e.GetEpisodeGuestsAsync(1, CancellationToken.None))
            .ReturnsAsync([]);
        _query.Setup(q => q.GetAllPlatformsAsync(CancellationToken.None))
            .ReturnsAsync([]);

        var result = await _controller.LogSocial(1);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<SocialPublishingViewModel>();
    }

    /// <summary>
    /// تسجيل Social_ Get_ Nonexistent Episode_ Returns View With Null.
    /// </summary>
    [Fact]
    public async Task LogSocial_Get_NonexistentEpisode_ReturnsViewWithNull()
    {
        _episodes.Setup(e => e.GetActiveEpisodeByIdAsync(999, CancellationToken.None))
            .ReturnsAsync((ActiveEpisodeDto?)null);

        var result = await _controller.LogSocial(999);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var vm = viewResult.Model as SocialPublishingViewModel;
        vm!.Episode.Should().BeNull();
    }

    /// <summary>
    /// تسجيل Social_ Post_ Valid_ Redirects To Index.
    /// </summary>
    [Fact]
    public async Task LogSocial_Post_Valid_RedirectsToIndex()
    {
        _command.Setup(c => c.LogSocialPublishingAsync(It.IsAny<int>(),
                It.IsAny<List<SocialMediaPublishingLogDto>>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result.Success());

        var form = TestDataFactory.CreateSocialForm(
            TestDataFactory.CreateGuestLogFormItem());

        var result = await _controller.LogSocial(1, form);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(PublishingController.Index));
    }

    /// <summary>
    /// تسجيل Social_ Post_ Failure_ Redirects Back.
    /// </summary>
    [Fact]
    public async Task LogSocial_Post_Failure_RedirectsBack()
    {
        _command.Setup(c => c.LogSocialPublishingAsync(It.IsAny<int>(),
                It.IsAny<List<SocialMediaPublishingLogDto>>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result.Fail("خطأ في النشر"));

        var form = TestDataFactory.CreateSocialForm(
            TestDataFactory.CreateGuestLogFormItem());

        var result = await _controller.LogSocial(1, form);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(PublishingController.LogSocial));
    }

    /// <summary>
    /// تعديل _ Get_ Existing Log_ Returns View.
    /// </summary>
    [Fact]
    public async Task Edit_Get_ExistingLog_ReturnsView()
    {
        var dto = TestDataFactory.CreateSocialLog(episodeGuestId: 1, episodeId: 1, title: "Existing");
        _query.Setup(q => q.GetSocialPublishingLogByIdAsync(1, CancellationToken.None))
            .ReturnsAsync(dto);
        _query.Setup(q => q.GetAllPlatformsAsync(CancellationToken.None))
            .ReturnsAsync([]);
        _query.Setup(q => q.GetEpisodeSocialLogsAsync(1, CancellationToken.None))
            .ReturnsAsync([]);
        _query.Setup(q => q.GetAllPublishingRecordsAsync(1, CancellationToken.None))
            .ReturnsAsync([]);
        _episodes.Setup(e => e.GetActiveEpisodeByIdAsync(1, CancellationToken.None))
            .ReturnsAsync(new ActiveEpisodeDto { EpisodeId = 1 });
        _episodes.Setup(e => e.GetEpisodeGuestsAsync(1, CancellationToken.None))
            .ReturnsAsync([]);

        var result = await _controller.Edit(1);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<SocialPublishingEditViewModel>();
    }

    /// <summary>
    /// تعديل _ Get_ Nonexistent Log_ Returns Not Found.
    /// </summary>
    [Fact]
    public async Task Edit_Get_NonexistentLog_ReturnsNotFound()
    {
        _query.Setup(q => q.GetSocialPublishingLogByIdAsync(999, CancellationToken.None))
            .ReturnsAsync((SocialMediaPublishingLogDto?)null);

        var result = await _controller.Edit(999);

        result.Should().BeOfType<NotFoundResult>();
    }

    /// <summary>
    /// تعديل _ Post_ Valid_ Redirects.
    /// </summary>
    [Fact]
    public async Task Edit_Post_Valid_Redirects()
    {
        _command.Setup(c => c.UpdateSocialPublishingLogAsync(It.IsAny<SocialMediaPublishingLogDto>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result.Success());

        var form = TestDataFactory.CreateSocialForm(
            TestDataFactory.CreateGuestLogFormItem(episodeGuestId: 1, episodeId: 1, logId: 1));

        var result = await _controller.Edit(1, form);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(PublishingController.Index));
    }

    /// <summary>
    /// تعديل _ Post_ No الضيف Log_ Returns Bad Request.
    /// </summary>
    [Fact]
    public async Task Edit_Post_NoGuestLog_ReturnsBadRequest()
    {
        var form = new SocialPublishingFormModel();

        var result = await _controller.Edit(1, form);

        result.Should().BeOfType<BadRequestResult>();
    }

    /// <summary>
    /// حذف _ Valid_ Redirects.
    /// </summary>
    [Fact]
    public async Task Delete_Valid_Redirects()
    {
        _command.Setup(c => c.DeleteSocialPublishingLogAsync(1, _admin, CancellationToken.None))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Delete(1);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(PublishingController.Index));
    }

    /// <summary>
    /// حذف _ Failure_ Redirects With Error.
    /// </summary>
    [Fact]
    public async Task Delete_Failure_RedirectsWithError()
    {
        _command.Setup(c => c.DeleteSocialPublishingLogAsync(1, _admin, CancellationToken.None))
            .ReturnsAsync(Result.Fail("لا يمكن الحذف"));

        var result = await _controller.Delete(1);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(PublishingController.Index));
        _controller.TempData["Error"].Should().Be("لا يمكن الحذف");
    }
}
