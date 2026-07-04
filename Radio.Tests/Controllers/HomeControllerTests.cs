// ============================================================
// HomeControllerTests — اختبارات الصفحة الرئيسية
// ============================================================
// المسؤولية: تعريف اختبارات الصفحة الرئيسية.
// ============================================================
using DataAccess.DTOs;
using DataAccess.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Radio.Web.Controllers;
using Radio.Web.ViewModels;

namespace Radio.Tests.Controllers;

/// <summary>
/// صنف اختبارات الصفحة الرئيسية.
/// </summary>
public class HomeControllerTests
{
    private readonly Mock<IReportsService> _reports = new();
    private readonly Mock<IEpisodeQueryService> _episodeQuery = new();
    private readonly HomeController _controller;

    public HomeControllerTests()
    {
        _controller = new HomeController(_reports.Object, _episodeQuery.Object, Mock.Of<ILogger<HomeController>>());
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    /// <summary>
    /// عرض قائمة _ Returns View With Dashboard.
    /// </summary>
    [Fact]
    public async Task Index_ReturnsViewWithDashboard()
    {
        _reports.Setup(r => r.GetTodayEpisodesAsync(CancellationToken.None)).ReturnsAsync([]);
        _reports.Setup(r => r.GetEpisodeStatusStatsAsync(CancellationToken.None)).ReturnsAsync(new Dictionary<string, int>());
        _reports.Setup(r => r.GetMostActiveProgramsAsync(CancellationToken.None)).ReturnsAsync([]);
        _reports.Setup(r => r.GetTopGuestsAsync(10, CancellationToken.None)).ReturnsAsync([]);
        _reports.Setup(r => r.GetCancelledEpisodesAsync(CancellationToken.None)).ReturnsAsync([]);

        var result = await _controller.Index();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<DashboardViewModel>();
    }

    /// <summary>
    /// عرض قائمة _ Exception_ Returns Error View.
    /// </summary>
    [Fact]
    public async Task Index_Exception_ReturnsErrorView()
    {
        _reports.Setup(r => r.GetTodayEpisodesAsync(CancellationToken.None))
            .ThrowsAsync(new Exception("test"));

        var result = await _controller.Index();

        result.Should().BeOfType<ViewResult>().Subject.ViewName.Should().Be("Error");
    }

    /// <summary>
    /// Error_ Returns View.
    /// </summary>
    [Fact]
    public void Error_ReturnsView()
    {
        var result = _controller.Error();

        result.Should().BeOfType<ViewResult>();
    }
}
