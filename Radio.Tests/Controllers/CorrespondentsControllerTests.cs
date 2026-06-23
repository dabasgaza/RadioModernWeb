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

public class CorrespondentsControllerTests
{
    private readonly Mock<ICorrespondentService> _correspondents = new();
    private readonly Mock<ICurrentUserService> _currentUser;
    private readonly CorrespondentsController _controller;
    private readonly UserSession _admin = UserSessionBuilder.CreateAdmin();

    public CorrespondentsControllerTests()
    {
        _currentUser = UserSessionBuilder.CreateMock(_admin);
        _controller = new CorrespondentsController(_correspondents.Object, _currentUser.Object, Mock.Of<ILogger<CorrespondentsController>>());
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }

    [Fact]
    public async Task Index_ReturnsViewWithCorrespondents()
    {
        _correspondents.Setup(c => c.GetAllActiveAsync(CancellationToken.None))
            .ReturnsAsync([new CorrespondentDto(1, "مراسل", "010000", null)]);

        var result = await _controller.Index(null);

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Index_Search_ReturnsFiltered()
    {
        _correspondents.Setup(c => c.GetAllActiveAsync(CancellationToken.None))
            .ReturnsAsync([
                new CorrespondentDto(1, "أحمد", "010000", null),
                new CorrespondentDto(2, "محمد", "020000", null)
            ]);

        var result = await _controller.Index("أحمد");

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model as List<CorrespondentDto>;
        model.Should().HaveCount(1);
    }

    [Fact]
    public void Create_Get_ReturnsEditView()
    {
        var result = _controller.Create();

        result.Should().BeOfType<ViewResult>().Subject.ViewName.Should().Be("Edit");
    }

    [Fact]
    public async Task Create_Post_Valid_Redirects()
    {
        _correspondents.Setup(c => c.CreateAsync(It.IsAny<CorrespondentDto>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result<int>.Success(1));

        var model = new CorrespondentDto(0, "مراسل جديد", "010000", null);
        var result = await _controller.Create(model);

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Create_Post_InvalidModel_ReturnsView()
    {
        _controller.ModelState.AddModelError("FullName", "مطلوب");
        var model = new CorrespondentDto(0, "", null, null);

        var result = await _controller.Create(model);

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Create_Post_Failure_StaysOnEdit()
    {
        _correspondents.Setup(c => c.CreateAsync(It.IsAny<CorrespondentDto>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result<int>.Fail("خطأ"));

        var model = new CorrespondentDto(0, "مراسل", "010000", null);
        var result = await _controller.Create(model);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewData.ModelState.ErrorCount.Should().BePositive();
    }

    [Fact]
    public async Task Edit_Get_Existing_ReturnsView()
    {
        _correspondents.Setup(c => c.GetAllActiveAsync(CancellationToken.None))
            .ReturnsAsync([new CorrespondentDto(4, "موجود", "010000", null)]);

        var result = await _controller.Edit(4);

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Edit_Get_NonExisting_ReturnsNotFound()
    {
        _correspondents.Setup(c => c.GetAllActiveAsync(CancellationToken.None))
            .ReturnsAsync([]);

        var result = await _controller.Edit(999);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Edit_Post_Valid_Redirects()
    {
        _correspondents.Setup(c => c.UpdateAsync(It.IsAny<CorrespondentDto>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result.Success());

        var model = new CorrespondentDto(4, "مُحدّث", "010000", null);
        var result = await _controller.Edit(4, model);

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Edit_Post_InvalidModel_ReturnsView()
    {
        _controller.ModelState.AddModelError("FullName", "مطلوب");
        var model = new CorrespondentDto(4, "", null, null);

        var result = await _controller.Edit(4, model);

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Edit_Post_Failure_StaysOnEdit()
    {
        _correspondents.Setup(c => c.UpdateAsync(It.IsAny<CorrespondentDto>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result.Fail("خطأ"));

        var model = new CorrespondentDto(4, "مراسل", "010000", null);
        var result = await _controller.Edit(4, model);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewData.ModelState.ErrorCount.Should().BePositive();
    }

    [Fact]
    public async Task Delete_Valid_Redirects()
    {
        _correspondents.Setup(c => c.SoftDeleteAsync(1, _admin, CancellationToken.None))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Delete(1);

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Delete_Failure_RedirectsWithError()
    {
        _correspondents.Setup(c => c.SoftDeleteAsync(1, _admin, CancellationToken.None))
            .ReturnsAsync(Result.Fail("لا يمكن الحذف"));

        await _controller.Delete(1);

        _controller.TempData["Error"].Should().Be("لا يمكن الحذف");
    }

    [Fact]
    public async Task Coverage_ReturnsView()
    {
        _correspondents.Setup(c => c.GetCoverageAsync(1, CancellationToken.None))
            .ReturnsAsync([]);

        var result = await _controller.Coverage(1);

        result.Should().BeOfType<ViewResult>();
    }
}
