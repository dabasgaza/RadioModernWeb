// ============================================================
// ProgramsControllerTests — اختبارات البرامج
// ============================================================
// المسؤولية: تعريف اختبارات البرامج.
// ============================================================
using DataAccess.Common;
using DataAccess.DTOs;
using DataAccess.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Radio.Tests.TestData.Builders;
using Radio.Web.Controllers;
using Radio.Web.Services;

namespace Radio.Tests.Controllers;

/// <summary>
/// صنف اختبارات البرامج.
/// </summary>
public class ProgramsControllerTests
{
    private readonly Mock<IProgramService> _programs = new();
    private readonly Mock<IEpisodeService> _episodes = new();
    private readonly Mock<ICurrentUserService> _currentUser;
    private readonly ProgramsController _controller;
    private readonly UserSession _admin = UserSessionBuilder.CreateAdmin();

    public ProgramsControllerTests()
    {
        _currentUser = UserSessionBuilder.CreateMock(_admin);
        _controller = new ProgramsController(_programs.Object, _episodes.Object, _currentUser.Object, Mock.Of<ILogger<ProgramsController>>());
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }

    /// <summary>
    /// عرض قائمة _ Returns View.
    /// </summary>
    [Fact]
    public async Task Index_ReturnsView()
    {
        _programs.Setup(p => p.GetAllActiveAsync(CancellationToken.None))
            .ReturnsAsync([new ProgramDto(1, "برنامج", null, null)]);
        _episodes.Setup(e => e.GetActiveEpisodesAsync(CancellationToken.None))
            .ReturnsAsync([]);

        var result = await _controller.Index(null);

        result.Should().BeOfType<ViewResult>();
    }

    /// <summary>
    /// عرض قائمة _ Exception_ Returns Error View.
    /// </summary>
    [Fact]
    public async Task Index_Exception_ReturnsErrorView()
    {
        _programs.Setup(p => p.GetAllActiveAsync(CancellationToken.None))
            .ThrowsAsync(new Exception("test"));

        var result = await _controller.Index(null);

        result.Should().BeOfType<ViewResult>().Subject.ViewName.Should().Be("Error");
    }

    /// <summary>
    /// إنشاء _ Get_ Returns Edit View.
    /// </summary>
    [Fact]
    public void Create_Get_ReturnsEditView()
    {
        var result = _controller.Create();

        result.Should().BeOfType<ViewResult>().Subject.ViewName.Should().Be("Edit");
    }

    /// <summary>
    /// إنشاء _ Post_ Valid_ Redirects.
    /// </summary>
    [Fact]
    public async Task Create_Post_Valid_Redirects()
    {
        _programs.Setup(p => p.CreateProgramAsync(It.IsAny<ProgramDto>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result<int>.Success(1));

        var model = new ProgramDto(0, "برنامج جديد", null, null);
        var result = await _controller.Create(model);

        result.Should().BeOfType<RedirectToActionResult>();
    }

    /// <summary>
    /// إنشاء _ Post_ Empty Name_ Adds نموذج Error.
    /// </summary>
    [Fact]
    public async Task Create_Post_EmptyName_AddsModelError()
    {
        var model = new ProgramDto(0, "", null, null);

        await _controller.Create(model);

        _controller.ModelState["ProgramName"]?.Errors.Should().NotBeEmpty();
    }

    /// <summary>
    /// إنشاء _ Post_ Failure_ Stays On Edit.
    /// </summary>
    [Fact]
    public async Task Create_Post_Failure_StaysOnEdit()
    {
        _programs.Setup(p => p.CreateProgramAsync(It.IsAny<ProgramDto>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result<int>.Fail("خطأ"));

        var model = new ProgramDto(0, "برنامج", null, null);
        var result = await _controller.Create(model);

        result.Should().BeOfType<ViewResult>().Subject.ViewName.Should().Be("Edit");
    }

    /// <summary>
    /// تعديل _ Get_ Existing_ Returns View.
    /// </summary>
    [Fact]
    public async Task Edit_Get_Existing_ReturnsView()
    {
        _programs.Setup(p => p.GetAllActiveAsync(CancellationToken.None))
            .ReturnsAsync([new ProgramDto(3, "موجود", null, null)]);

        var result = await _controller.Edit(3);

        result.Should().BeOfType<ViewResult>();
    }

    /// <summary>
    /// تعديل _ Get_ Non Existing_ Returns Not Found.
    /// </summary>
    [Fact]
    public async Task Edit_Get_NonExisting_ReturnsNotFound()
    {
        _programs.Setup(p => p.GetAllActiveAsync(CancellationToken.None))
            .ReturnsAsync([]);

        var result = await _controller.Edit(999);

        result.Should().BeOfType<NotFoundResult>();
    }

    /// <summary>
    /// تعديل _ Post_ Valid_ Redirects.
    /// </summary>
    [Fact]
    public async Task Edit_Post_Valid_Redirects()
    {
        _programs.Setup(p => p.UpdateProgramAsync(It.IsAny<ProgramDto>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result.Success());

        var model = new ProgramDto(3, "مُحدّث", null, null);
        var result = await _controller.Edit(3, model);

        result.Should().BeOfType<RedirectToActionResult>();
    }

    /// <summary>
    /// تعديل _ Post_ Empty Name_ Adds نموذج Error.
    /// </summary>
    [Fact]
    public async Task Edit_Post_EmptyName_AddsModelError()
    {
        var model = new ProgramDto(3, "", null, null);

        await _controller.Edit(3, model);

        _controller.ModelState["ProgramName"]?.Errors.Should().NotBeEmpty();
    }

    /// <summary>
    /// حذف _ Valid_ Redirects.
    /// </summary>
    [Fact]
    public async Task Delete_Valid_Redirects()
    {
        _programs.Setup(p => p.SoftDeleteAsync(1, _admin, CancellationToken.None))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Delete(1);

        result.Should().BeOfType<RedirectToActionResult>();
    }

    /// <summary>
    /// حذف _ Failure_ Redirects With Error.
    /// </summary>
    [Fact]
    public async Task Delete_Failure_RedirectsWithError()
    {
        _programs.Setup(p => p.SoftDeleteAsync(1, _admin, CancellationToken.None))
            .ReturnsAsync(Result.Fail("لا يمكن الحذف"));

        await _controller.Delete(1);

        _controller.TempData["Error"].Should().Be("لا يمكن الحذف");
    }
}
