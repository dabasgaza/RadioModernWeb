using DataAccess.Common;
using DataAccess.DTOs;
using DataAccess.Services;
using Domain.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Radio.Tests.Helpers;
using Radio.Tests.TestData.Builders;
using Radio.Web.Controllers;
using Radio.Web.Services;
using Radio.Web.ViewModels;

namespace Radio.Tests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _users = new();
    private readonly Mock<ICurrentUserService> _currentUser;
    private readonly UsersController _controller;
    private readonly UserSession _admin = UserSessionBuilder.CreateAdmin();

    public UsersControllerTests()
    {
        _currentUser = UserSessionBuilder.CreateMock(_admin);
        _controller = new UsersController(_users.Object, _currentUser.Object, Mock.Of<ILogger<UsersController>>(), ValidValidator.Create<UserDto>());
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }

    [Fact]
    public async Task Index_ReturnsView()
    {
        _users.Setup(u => u.GetAllUsersAsync(CancellationToken.None)).ReturnsAsync([]);

        var result = await _controller.Index(null);

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Create_Get_ReturnsEditView()
    {
        _users.Setup(u => u.GetRolesAsync(CancellationToken.None)).ReturnsAsync([]);

        var result = await _controller.Create();

        result.Should().BeOfType<ViewResult>().Subject.ViewName.Should().Be("Edit");
    }

    [Fact]
    public async Task Create_Post_Valid_Redirects()
    {
        _users.Setup(u => u.GetRolesAsync(CancellationToken.None)).ReturnsAsync([]);
        _users.Setup(u => u.CreateUserAsync(It.IsAny<UserDto>(), It.IsAny<string>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result<int>.Success(1));

        var result = await _controller.Create(new UserViewModel { FullName = "مستخدم", Username = "user", Password = "pass123", RoleId = 1 });

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Create_Post_Failure_ReturnsView()
    {
        _users.Setup(u => u.GetRolesAsync(CancellationToken.None)).ReturnsAsync([]);
        _users.Setup(u => u.CreateUserAsync(It.IsAny<UserDto>(), It.IsAny<string>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result<int>.Fail("خطأ"));

        var result = await _controller.Create(new UserViewModel { FullName = "مستخدم", Username = "user", Password = "pass123", RoleId = 1 });

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Edit_Get_Existing_ReturnsView()
    {
        _users.Setup(u => u.GetAllUsersAsync(CancellationToken.None))
            .ReturnsAsync([new UserDto { UserId = 3, FullName = "موجود", Username = "user", RoleId = 1 }]);
        _users.Setup(u => u.GetRolesAsync(CancellationToken.None)).ReturnsAsync([]);

        var result = await _controller.Edit(3);

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Edit_Get_NonExisting_ReturnsNotFound()
    {
        _users.Setup(u => u.GetAllUsersAsync(CancellationToken.None)).ReturnsAsync([]);

        var result = await _controller.Edit(999);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Edit_Post_Valid_Redirects()
    {
        _users.Setup(u => u.GetRolesAsync(CancellationToken.None)).ReturnsAsync([]);
        _users.Setup(u => u.UpdateUserAsync(It.IsAny<UserDto>(), It.IsAny<string?>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Edit(3, new UserViewModel { FullName = "محدّث", Username = "user", RoleId = 1 });

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task ToggleStatus_Existing_Redirects()
    {
        _users.Setup(u => u.GetAllUsersAsync(CancellationToken.None))
            .ReturnsAsync([new UserDto { UserId = 5, FullName = "موجود", Username = "user", RoleId = 1 }]);
        _users.Setup(u => u.ToggleUserStatusAsync(5, true, _admin, CancellationToken.None))
            .ReturnsAsync(Result.Success());

        var result = await _controller.ToggleStatus(5);

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task ToggleStatus_NonExisting_ReturnsNotFound()
    {
        _users.Setup(u => u.GetAllUsersAsync(CancellationToken.None)).ReturnsAsync([]);

        var result = await _controller.ToggleStatus(999);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_Valid_Redirects()
    {
        _users.Setup(u => u.DeleteUserAsync(1, _admin, CancellationToken.None))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Delete(1);

        result.Should().BeOfType<RedirectToActionResult>();
    }
}

public class RolesControllerTests
{
    private readonly Mock<IUserService> _users = new();
    private readonly Mock<ICurrentUserService> _currentUser;
    private readonly RolesController _controller;
    private readonly UserSession _admin = UserSessionBuilder.CreateAdmin();

    public RolesControllerTests()
    {
        _currentUser = UserSessionBuilder.CreateMock(_admin);
        _controller = new RolesController(_users.Object, _currentUser.Object);
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }

    [Fact]
    public async Task Index_ReturnsView()
    {
        _users.Setup(u => u.GetRolesAsync(CancellationToken.None)).ReturnsAsync([]);

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
        _users.Setup(u => u.CreateRoleAsync(It.IsAny<RoleDto>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result<int>.Success(1));

        var result = await _controller.Create(new RoleDto { RoleName = "دور" });

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Create_Post_EmptyName_ReturnsView()
    {
        var result = await _controller.Create(new RoleDto { RoleName = "" });

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Edit_Get_Existing_ReturnsView()
    {
        _users.Setup(u => u.GetRolesAsync(CancellationToken.None))
            .ReturnsAsync([new RoleDto { RoleId = 2, RoleName = "موجود" }]);

        var result = await _controller.Edit(2);

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Edit_Get_NonExisting_ReturnsNotFound()
    {
        _users.Setup(u => u.GetRolesAsync(CancellationToken.None)).ReturnsAsync([]);

        var result = await _controller.Edit(999);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Edit_Post_Valid_Redirects()
    {
        _users.Setup(u => u.UpdateRoleAsync(It.IsAny<RoleDto>(), _admin, CancellationToken.None))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Edit(2, new RoleDto { RoleName = "مُحدّث" });

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Delete_Valid_Redirects()
    {
        _users.Setup(u => u.DeleteRoleAsync(1, _admin, CancellationToken.None))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Delete(1);

        result.Should().BeOfType<RedirectToActionResult>();
    }
}

public class AuditLogsControllerTests
{
    private readonly Mock<IAuditLogService> _auditLog = new();
    private readonly AuditLogsController _controller;

    public AuditLogsControllerTests()
    {
        _controller = new AuditLogsController(_auditLog.Object);
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    [Fact]
    public async Task Index_ReturnsView()
    {
        _auditLog.Setup(a => a.GetFilteredAuditLogsAsync(null, null, null, null, null, 1, 100, CancellationToken.None))
            .ReturnsAsync(Result<PagedAuditLogResult>.Success(new PagedAuditLogResult { Items = [], TotalCount = 0 }));

        var result = await _controller.Index(null, null, null, null);

        result.Should().BeOfType<ViewResult>();
    }
}
