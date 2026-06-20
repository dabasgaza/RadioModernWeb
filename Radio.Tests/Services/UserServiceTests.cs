using DataAccess.Common;
using DataAccess.DTOs;
using DataAccess.Services;
using Domain.Models;
using Radio.Tests.Helpers;
using Radio.Tests.TestData.Builders;
using Radio.Tests.TestData.Fixtures;

namespace Radio.Tests.Services;

public class UserServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _db;
    private readonly IUserService _service;
    private readonly UserSession _admin = UserSessionBuilder.CreateAdmin();

    public UserServiceTests(DatabaseFixture db)
    {
        _db = db;
        var sp = new Mock<IServiceProvider>();
        var sessionProvider = new CurrentSessionProvider(sp.Object);
        _service = new UserService(db.DbContextFactory, sessionProvider);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsUsers()
    {
        var result = await _service.GetAllUsersAsync();
        result.Should().Contain(u => u.Username == "admin");
    }

    [Fact]
    public async Task CreateUserAsync_Valid_ReturnsSuccess()
    {
        var dto = new UserDto
        {
            Username = "newuser",
            FullName = "New User",
            RoleName = "Operator"
        };
        var result = await _service.CreateUserAsync(dto, "Password123!", _admin);
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task CreateUserAsync_DuplicateUsername_ReturnsFail()
    {
        var dto = new UserDto
        {
            Username = "admin",
            FullName = "Admin Copy",
            RoleName = "Operator"
        };
        var result = await _service.CreateUserAsync(dto, "Password123!", _admin);
        result.ShouldBeFailure("موجود");
    }

    [Fact]
    public async Task ToggleUserStatusAsync_DeactivatesUser()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.Users.Add(new Domain.Models.User
        {
            UserId = 100, Username = "target", FullName = "Target User",
            PasswordHash = "hash", EmailAddress = "", PhoneNumber = "",
            RoleId = 1, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var result = await _service.ToggleUserStatusAsync(100, false, _admin);
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task GetRolesAsync_ReturnsRoles()
    {
        var result = await _service.GetRolesAsync();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateRoleAsync_Valid_ReturnsSuccess()
    {
        var dto = new RoleDto { RoleName = "Editor", RoleDescription = "تحرير المحتوى" };
        var result = await _service.CreateRoleAsync(dto, _admin);
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task UpdateRoleAsync_Valid_ReturnsSuccess()
    {
        var dto = new RoleDto { RoleId = 1, RoleName = "SuperAdmin", RoleDescription = "مسؤول كامل الصلاحيات" };
        var result = await _service.UpdateRoleAsync(dto, _admin);
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task GetPermissionsMatrixAsync_ReturnsPermissions()
    {
        var result = await _service.GetPermissionsMatrixAsync(1);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateRolePermissionsAsync_Valid_ReturnsSuccess()
    {
        var result = await _service.UpdateRolePermissionsAsync(1, [], _admin);
        result.ShouldBeSuccess();
    }
}
