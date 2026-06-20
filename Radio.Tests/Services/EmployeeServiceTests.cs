using DataAccess.Common;
using DataAccess.DTOs;
using DataAccess.Services;
using Domain.Models;
using Radio.Tests.Helpers;
using Radio.Tests.TestData.Builders;
using Radio.Tests.TestData.Fixtures;
using Microsoft.Extensions.Logging;

namespace Radio.Tests.Services;

public class EmployeeServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _db;
    private readonly IEmployeeService _service;
    private readonly UserSession _admin = UserSessionBuilder.CreateAdmin();

    public EmployeeServiceTests(DatabaseFixture db)
    {
        _db = db;
        var lookup = Mock.Of<ICachedLookupService>();
        _service = new EmployeeService(db.DbContextFactory, lookup, Mock.Of<ILogger<EmployeeService>>());
    }

    [Fact]
    public async Task GetAllActiveAsync_ReturnsAll()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.Employees.Add(new Employee { FullName = "Emp1", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await ctx.SaveChangesAsync();

        var result = await _service.GetAllActiveAsync();

        result.Should().Contain(e => e.FullName == "Emp1");
    }

    [Fact]
    public async Task CreateAsync_Valid_ReturnsSuccess()
    {
        var dto = new EmployeeDto(0, "New Emp", StaffRoleId: 1, null, null);

        var result = await _service.CreateAsync(dto, _admin);

        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task GetAllRolesAsync_ReturnsRoles()
    {
        var result = await _service.GetAllRolesAsync();
        result.Should().Contain(r => r.RoleName == "مذيع");
    }

    [Fact]
    public async Task CreateRoleAsync_Valid_ReturnsSuccess()
    {
        var dto = new StaffRoleDto(0, "مصور");

        var result = await _service.CreateRoleAsync(dto, _admin);

        result.ShouldBeSuccess();
    }
}
