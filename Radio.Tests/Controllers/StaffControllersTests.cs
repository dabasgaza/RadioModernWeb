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

public class EmployeesControllerTests
{
    private readonly Mock<IEmployeeService> _employees = new();
    private readonly Mock<ICurrentUserService> _currentUser;
    private readonly EmployeesController _controller;
    private readonly UserSession _admin = UserSessionBuilder.CreateAdmin();

    public EmployeesControllerTests()
    {
        _currentUser = UserSessionBuilder.CreateMock(_admin);
        _controller = new EmployeesController(_employees.Object, _currentUser.Object, Mock.Of<ILogger<EmployeesController>>());
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }

    [Fact]
    public async Task Index_ReturnsView()
    {
        _employees.Setup(e => e.GetAllActiveAsync(CancellationToken.None)).ReturnsAsync([]);

        var result = await _controller.Index(null);

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Create_Get_ReturnsEditView()
    {
        _employees.Setup(e => e.GetAllRolesAsync(CancellationToken.None)).ReturnsAsync([]);

        var result = await _controller.Create();

        result.Should().BeOfType<ViewResult>().Subject.ViewName.Should().Be("Edit");
    }

    [Fact]
    public async Task Create_Post_Valid_Redirects()
    {
        _employees.Setup(e => e.CreateAsync(It.IsAny<EmployeeDto>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result<int>.Success(1));

        var result = await _controller.Create(new EmployeeDto(0, "موظف", 1, null, null));

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Create_Post_InvalidModel_ReturnsView()
    {
        _controller.ModelState.AddModelError("FullName", "مطلوب");
        _employees.Setup(e => e.GetAllRolesAsync(CancellationToken.None)).ReturnsAsync([]);

        var result = await _controller.Create(new EmployeeDto(0, "", null, null, null));

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Create_Post_Failure_ReturnsView()
    {
        _employees.Setup(e => e.CreateAsync(It.IsAny<EmployeeDto>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result<int>.Fail("خطأ"));
        _employees.Setup(e => e.GetAllRolesAsync(CancellationToken.None)).ReturnsAsync([]);

        var result = await _controller.Create(new EmployeeDto(0, "موظف", 1, null, null));

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Edit_Get_Existing_ReturnsView()
    {
        _employees.Setup(e => e.GetAllActiveAsync(CancellationToken.None))
            .ReturnsAsync([new EmployeeDto(5, "موجود", 1, null, null)]);
        _employees.Setup(e => e.GetAllRolesAsync(CancellationToken.None)).ReturnsAsync([]);

        var result = await _controller.Edit(5);

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Edit_Get_NonExisting_ReturnsNotFound()
    {
        _employees.Setup(e => e.GetAllActiveAsync(CancellationToken.None)).ReturnsAsync([]);

        var result = await _controller.Edit(999);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Edit_Post_Valid_Redirects()
    {
        _employees.Setup(e => e.UpdateAsync(It.IsAny<EmployeeDto>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Edit(5, new EmployeeDto(5, "مُحدّث", 1, null, null));

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Delete_Valid_Redirects()
    {
        _employees.Setup(e => e.SoftDeleteAsync(1, _admin, CancellationToken.None))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Delete(1);

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Delete_Failure_RedirectsWithError()
    {
        _employees.Setup(e => e.SoftDeleteAsync(1, _admin, CancellationToken.None))
            .ReturnsAsync(Result.Fail("لا يمكن الحذف"));

        await _controller.Delete(1);

        _controller.TempData["Error"].Should().Be("لا يمكن الحذف");
    }
}

public class StaffRolesControllerTests
{
    private readonly Mock<IEmployeeService> _employees = new();
    private readonly Mock<ICurrentUserService> _currentUser;
    private readonly StaffRolesController _controller;
    private readonly UserSession _admin = UserSessionBuilder.CreateAdmin();

    public StaffRolesControllerTests()
    {
        _currentUser = UserSessionBuilder.CreateMock(_admin);
        _controller = new StaffRolesController(_employees.Object, _currentUser.Object);
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }

    [Fact]
    public async Task Index_ReturnsView()
    {
        _employees.Setup(e => e.GetAllRolesAsync(CancellationToken.None)).ReturnsAsync([]);

        var result = await _controller.Index();

        result.Should().BeOfType<ViewResult>();
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
        _employees.Setup(e => e.CreateRoleAsync(It.IsAny<StaffRoleDto>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result<int>.Success(1));

        var result = await _controller.Create(new StaffRoleDto(0, "دور"));

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Create_Post_Failure_ReturnsView()
    {
        _employees.Setup(e => e.CreateRoleAsync(It.IsAny<StaffRoleDto>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result<int>.Fail("خطأ"));

        var result = await _controller.Create(new StaffRoleDto(0, "دور"));

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Edit_Get_Existing_ReturnsView()
    {
        _employees.Setup(e => e.GetAllRolesAsync(CancellationToken.None))
            .ReturnsAsync([new StaffRoleDto(3, "موجود")]);

        var result = await _controller.Edit(3);

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Edit_Get_NonExisting_ReturnsNotFound()
    {
        _employees.Setup(e => e.GetAllRolesAsync(CancellationToken.None)).ReturnsAsync([]);

        var result = await _controller.Edit(999);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Edit_Post_Valid_Redirects()
    {
        _employees.Setup(e => e.UpdateRoleAsync(It.IsAny<StaffRoleDto>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Edit(3, new StaffRoleDto(3, "مُحدّث"));

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Delete_Valid_Redirects()
    {
        _employees.Setup(e => e.SoftDeleteRoleAsync(1, _admin, CancellationToken.None))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Delete(1);

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Delete_Failure_RedirectsWithError()
    {
        _employees.Setup(e => e.SoftDeleteRoleAsync(1, _admin, CancellationToken.None))
            .ReturnsAsync(Result.Fail("لا يمكن الحذف"));

        await _controller.Delete(1);

        _controller.TempData["Error"].Should().Be("لا يمكن الحذف");
    }
}

public class SocialPlatformsControllerTests
{
    private readonly Mock<IPlatformService> _platforms = new();
    private readonly Mock<ICurrentUserService> _currentUser;
    private readonly SocialPlatformsController _controller;
    private readonly UserSession _admin = UserSessionBuilder.CreateAdmin();

    public SocialPlatformsControllerTests()
    {
        _currentUser = UserSessionBuilder.CreateMock(_admin);
        _controller = new SocialPlatformsController(_platforms.Object, _currentUser.Object);
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }

    [Fact]
    public async Task Index_ReturnsView()
    {
        _platforms.Setup(p => p.GetAllActiveAsync(CancellationToken.None)).ReturnsAsync([]);

        var result = await _controller.Index();

        result.Should().BeOfType<ViewResult>();
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
        _platforms.Setup(p => p.CreateAsync(It.IsAny<SocialMediaPlatformDto>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result<int>.Success(1));

        var result = await _controller.Create(new SocialMediaPlatformDto(0, "منصة", null));

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Create_Post_InvalidModel_ReturnsView()
    {
        _controller.ModelState.AddModelError("Name", "مطلوب");

        var result = await _controller.Create(new SocialMediaPlatformDto(0, "", null));

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Edit_Get_Existing_ReturnsView()
    {
        _platforms.Setup(p => p.GetAllActiveAsync(CancellationToken.None))
            .ReturnsAsync([new SocialMediaPlatformDto(4, "موجود", null)]);

        var result = await _controller.Edit(4);

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Edit_Get_NonExisting_ReturnsNotFound()
    {
        _platforms.Setup(p => p.GetAllActiveAsync(CancellationToken.None)).ReturnsAsync([]);

        var result = await _controller.Edit(999);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Edit_Post_Valid_Redirects()
    {
        _platforms.Setup(p => p.UpdateAsync(It.IsAny<SocialMediaPlatformDto>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Edit(4, new SocialMediaPlatformDto(4, "مُحدّث", null));

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Delete_Valid_Redirects()
    {
        _platforms.Setup(p => p.DeleteAsync(1, _admin, CancellationToken.None))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Delete(1);

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Delete_Failure_RedirectsWithError()
    {
        _platforms.Setup(p => p.DeleteAsync(1, _admin, CancellationToken.None))
            .ReturnsAsync(Result.Fail("لا يمكن الحذف"));

        await _controller.Delete(1);

        _controller.TempData["Error"].Should().Be("لا يمكن الحذف");
    }
}
