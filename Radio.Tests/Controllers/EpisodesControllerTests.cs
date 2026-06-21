using DataAccess.Common;
using DataAccess.DTOs;
using DataAccess.Services;
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

public class EpisodesControllerTests
{
    private readonly Mock<IEpisodeQueryService> _query = new();
    private readonly Mock<IEpisodeCommandService> _command = new();
    private readonly Mock<IExecutionService> _execution = new();
    private readonly Mock<IPublishingQueryService> _publishing = new();
    private readonly Mock<ICachedLookupService> _lookup = new();
    private readonly Mock<ICurrentUserService> _currentUser;
    private readonly Mock<ILogger<EpisodesController>> _logger = new();
    private readonly EpisodesController _controller;
    private readonly UserSession _admin = UserSessionBuilder.CreateAdmin();

    public EpisodesControllerTests()
    {
        _currentUser = UserSessionBuilder.CreateMock(_admin);
        _controller = new EpisodesController(
            _query.Object, _command.Object, _execution.Object,
            _publishing.Object, _lookup.Object, _currentUser.Object, _logger.Object);

        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }

    [Fact]
    public async Task Index_ReturnsViewWithEpisodeListViewModel()
    {
        _query.Setup(q => q.GetActiveEpisodesAsync(CancellationToken.None)).ReturnsAsync([]);
        _lookup.Setup(l => l.GetProgramsAsync(CancellationToken.None)).ReturnsAsync([]);

        var result = await _controller.Index(null, null, null);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<EpisodeListViewModel>();
    }

    [Fact]
    public async Task Details_ExistingEpisode_ReturnsViewWithDetailsViewModel()
    {
        var ep = new ActiveEpisodeDto
        {
            EpisodeId = 1, EpisodeName = "Ep1", ProgramId = 1, ProgramName = "P1",
            StatusId = 0
        };
        _query.Setup(q => q.GetActiveEpisodeByIdAsync(1, CancellationToken.None)).ReturnsAsync(ep);
        _publishing.Setup(p => p.GetAllPublishingRecordsAsync(1, CancellationToken.None)).ReturnsAsync([]);

        var result = await _controller.Details(1);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<EpisodeDetailsViewModel>();
    }

    [Fact]
    public async Task Details_NotFound_ReturnsNotFound()
    {
        _query.Setup(q => q.GetActiveEpisodeByIdAsync(999, CancellationToken.None)).ReturnsAsync((ActiveEpisodeDto?)null);

        var result = await _controller.Details(999);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_Valid_StoresTempDataAndRedirects()
    {
        var form = new EpisodeEditFormModel
        {
            ProgramId = 1, EpisodeName = "New Ep",
            ScheduledDate = DateTime.UtcNow.AddDays(1)
        };
        var dto = form.ToDto();
        _lookup.Setup(l => l.GetProgramsAsync(CancellationToken.None)).ReturnsAsync([]);
        _command.Setup(c => c.CreateEpisodeAsync(It.IsAny<EpisodeDto>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result<int>.Success(1));

        var result = await _controller.Create(form);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Create_InvalidModel_ReturnsView()
    {
        _controller.ModelState.AddModelError("EpisodeName", "مطلوب");
        var form = new EpisodeEditFormModel();
        _lookup.Setup(l => l.GetProgramsAsync(CancellationToken.None)).ReturnsAsync([]);

        var result = await _controller.Create(form);

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Edit_Get_ExistingEpisode_ReturnsView()
    {
        _query.Setup(q => q.GetActiveEpisodeByIdAsync(1, CancellationToken.None))
            .ReturnsAsync(new ActiveEpisodeDto { EpisodeId = 1, ProgramId = 1, EpisodeName = "EditMe" });
        _lookup.Setup(l => l.GetProgramsAsync(CancellationToken.None)).ReturnsAsync([]);

        var result = await _controller.Edit(1);

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Edit_Post_ValidForm_Redirects()
    {
        var form = new EpisodeEditFormModel
        {
            EpisodeId = 1, ProgramId = 1, EpisodeName = "Updated",
            ScheduledDate = DateTime.UtcNow.AddDays(1)
        };
        _lookup.Setup(l => l.GetProgramsAsync(CancellationToken.None)).ReturnsAsync([]);
        _command.Setup(c => c.UpdateEpisodeAsync(It.IsAny<EpisodeDto>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Edit(1, form);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Delete_Existing_Redirects()
    {
        _command.Setup(c => c.DeleteEpisodeAsync(1, _admin, CancellationToken.None))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Delete(1);

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Execute_Valid_Redirects()
    {
        _execution.Setup(e => e.LogExecutionAsync(It.IsAny<ExecutionLogDto>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Execute(1, "Done", "None", 45);

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Cancel_Valid_Redirects()
    {
        _command.Setup(c => c.CancelEpisodeAsync(1, "reason", _admin, CancellationToken.None))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Cancel(1, "reason");

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task BatchDelete_WithIds_Redirects()
    {
        var result = await _controller.BatchDelete([1, 2, 3]);

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task BatchCancel_WithIds_Redirects()
    {
        var result = await _controller.BatchCancel([1, 2], "batch cancel");

        result.Should().BeOfType<RedirectToActionResult>();
    }
}
