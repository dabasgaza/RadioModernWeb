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

public class GuestsControllerTests
{
    private readonly Mock<IGuestService> _guests = new();
    private readonly Mock<ICurrentUserService> _currentUser;
    private readonly GuestsController _controller;
    private readonly UserSession _admin = UserSessionBuilder.CreateAdmin();

    public GuestsControllerTests()
    {
        _currentUser = UserSessionBuilder.CreateMock(_admin);
        _controller = new GuestsController(_guests.Object, _currentUser.Object, Mock.Of<ILogger<GuestsController>>());
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }

    [Fact]
    public async Task Index_ReturnsViewWithGuests()
    {
        _guests.Setup(g => g.GetAllActiveAsync(CancellationToken.None))
            .ReturnsAsync([new GuestDto(1, "ضيف", null, null, null, null, null)]);

        var result = await _controller.Index(null);

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Index_Search_ReturnsFiltered()
    {
        _guests.Setup(g => g.GetAllActiveAsync(CancellationToken.None))
            .ReturnsAsync([
                new GuestDto(1, "محمد", null, null, null, null, null),
                new GuestDto(2, "أحمد", "منظمة", null, null, null, null)
            ]);

        var result = await _controller.Index("أحمد");

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model as List<GuestDto>;
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
        _guests.Setup(g => g.CreateGuestAsync(It.IsAny<GuestDto>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result<int>.Success(1));

        var model = new GuestDto(0, "ضيف جديد", null, "010000", null, null, null);
        var result = await _controller.Create(model);

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Create_Post_InvalidModel_ReturnsView()
    {
        _controller.ModelState.AddModelError("FullName", "مطلوب");
        var model = new GuestDto(0, "", null, null, null, null, null);

        var result = await _controller.Create(model);

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Create_Post_Failure_StaysOnEdit()
    {
        _guests.Setup(g => g.CreateGuestAsync(It.IsAny<GuestDto>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result<int>.Fail("خطأ"));

        var model = new GuestDto(0, "ضيف", null, "010000", null, null, null);
        var result = await _controller.Create(model);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewData.ModelState.ErrorCount.Should().BePositive();
    }

    [Fact]
    public async Task Edit_Get_Existing_ReturnsView()
    {
        _guests.Setup(g => g.GetAllActiveAsync(CancellationToken.None))
            .ReturnsAsync([new GuestDto(5, "موجود", null, null, null, null, null)]);

        var result = await _controller.Edit(5);

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Edit_Get_NonExisting_ReturnsNotFound()
    {
        _guests.Setup(g => g.GetAllActiveAsync(CancellationToken.None))
            .ReturnsAsync([]);

        var result = await _controller.Edit(999);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Edit_Post_Valid_Redirects()
    {
        _guests.Setup(g => g.UpdateGuestAsync(It.IsAny<GuestDto>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result.Success());

        var model = new GuestDto(1, "مُحدّث", null, "010000", null, null, null);
        var result = await _controller.Edit(1, model);

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Edit_Post_InvalidModel_ReturnsView()
    {
        _controller.ModelState.AddModelError("FullName", "مطلوب");
        var model = new GuestDto(1, "", null, null, null, null, null);

        var result = await _controller.Edit(1, model);

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Edit_Post_Failure_StaysOnEdit()
    {
        _guests.Setup(g => g.UpdateGuestAsync(It.IsAny<GuestDto>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result.Fail("خطأ"));

        var model = new GuestDto(1, "ضيف", null, "010000", null, null, null);
        var result = await _controller.Edit(1, model);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewData.ModelState.ErrorCount.Should().BePositive();
    }

    [Fact]
    public async Task Delete_Valid_Redirects()
    {
        _guests.Setup(g => g.SoftDeleteGuestAsync(1, _admin, CancellationToken.None))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Delete(1);

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Delete_Failure_RedirectsWithError()
    {
        _guests.Setup(g => g.SoftDeleteGuestAsync(1, _admin, CancellationToken.None))
            .ReturnsAsync(Result.Fail("لا يمكن الحذف"));

        await _controller.Delete(1);

        _controller.TempData["Error"].Should().Be("لا يمكن الحذف");
    }
}
