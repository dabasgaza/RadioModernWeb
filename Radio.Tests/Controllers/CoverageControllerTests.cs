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

public class CoverageControllerTests
{
    private readonly Mock<ICoverageService> _coverage = new();
    private readonly Mock<ICorrespondentService> _correspondents = new();
    private readonly Mock<IGuestService> _guests = new();
    private readonly Mock<ICurrentUserService> _currentUser;
    private readonly CoverageController _controller;
    private readonly UserSession _admin = UserSessionBuilder.CreateAdmin();

    public CoverageControllerTests()
    {
        _currentUser = UserSessionBuilder.CreateMock(_admin);
        _controller = new CoverageController(_coverage.Object, _correspondents.Object, _guests.Object, _currentUser.Object, Mock.Of<ILogger<CoverageController>>());
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }

    [Fact]
    public async Task Index_ReturnsView()
    {
        _coverage.Setup(c => c.GetAllAsync(CancellationToken.None)).ReturnsAsync([]);
        _correspondents.Setup(c => c.GetAllActiveAsync(CancellationToken.None)).ReturnsAsync([]);
        _guests.Setup(g => g.GetAllActiveAsync(CancellationToken.None)).ReturnsAsync([]);

        var result = await _controller.Index();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Create_Get_ReturnsEditView()
    {
        _correspondents.Setup(c => c.GetAllActiveAsync(CancellationToken.None)).ReturnsAsync([]);
        _guests.Setup(g => g.GetAllActiveAsync(CancellationToken.None)).ReturnsAsync([]);

        var result = await _controller.Create();

        result.Should().BeOfType<ViewResult>().Subject.ViewName.Should().Be("Edit");
    }

    [Fact]
    public async Task Create_Post_Valid_Redirects()
    {
        _coverage.Setup(c => c.CreateAsync(It.IsAny<CoverageDto>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result<int>.Success(1));

        var result = await _controller.Create(new CoverageDto { CorrespondentId = 1 });

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Create_Post_InvalidModel_Redirects()
    {
        _controller.ModelState.AddModelError("Topic", "مطلوب");

        var result = await _controller.Create(new CoverageDto());

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Create_Post_Failure_Redirects()
    {
        _coverage.Setup(c => c.CreateAsync(It.IsAny<CoverageDto>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result<int>.Fail("خطأ"));

        var result = await _controller.Create(new CoverageDto { CorrespondentId = 1 });

        result.Should().BeOfType<RedirectToActionResult>();
        _controller.TempData["Error"].Should().Be("خطأ");
    }

    [Fact]
    public async Task Edit_Get_Existing_ReturnsView()
    {
        _coverage.Setup(c => c.GetByIdAsync(2, CancellationToken.None))
            .ReturnsAsync(new CoverageDto { CoverageId = 2, CorrespondentId = 1 });
        _correspondents.Setup(c => c.GetAllActiveAsync(CancellationToken.None)).ReturnsAsync([]);
        _guests.Setup(g => g.GetAllActiveAsync(CancellationToken.None)).ReturnsAsync([]);

        var result = await _controller.Edit(2);

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Edit_Get_NonExisting_ReturnsNotFound()
    {
        _coverage.Setup(c => c.GetByIdAsync(999, CancellationToken.None))
            .ReturnsAsync((CoverageDto?)null);

        var result = await _controller.Edit(999);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Edit_Post_Valid_Redirects()
    {
        _coverage.Setup(c => c.UpdateAsync(It.IsAny<CoverageDto>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Edit(2, new CoverageDto { CoverageId = 2, CorrespondentId = 1 });

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Edit_Post_InvalidModel_Redirects()
    {
        _controller.ModelState.AddModelError("Topic", "مطلوب");

        var result = await _controller.Edit(2, new CoverageDto());

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Delete_Valid_Redirects()
    {
        _coverage.Setup(c => c.DeleteAsync(1, _admin, CancellationToken.None))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Delete(1);

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Delete_Failure_RedirectsWithError()
    {
        _coverage.Setup(c => c.DeleteAsync(1, _admin, CancellationToken.None))
            .ReturnsAsync(Result.Fail("لا يمكن الحذف"));

        await _controller.Delete(1);

        _controller.TempData["Error"].Should().Be("لا يمكن الحذف");
    }
}
