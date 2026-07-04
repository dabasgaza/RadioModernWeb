// ============================================================
// EmployeeServiceTests — الموظف Service
// ============================================================
// المسؤولية: تعريف الموظف Service.
// ============================================================
using DataAccess.Common;
using DataAccess.DTOs;
using DataAccess.Services;
using Domain.Models;
using FluentValidation;
using Radio.Tests.Helpers;
using Radio.Tests.TestData.Builders;
using Radio.Tests.TestData.Fixtures;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Radio.Tests.Services;

/// <summary>
/// صنف الموظف Service.
/// </summary>
public class EmployeeServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _db;
    private readonly IEmployeeService _service;
    private readonly UserSession _admin = UserSessionBuilder.CreateAdmin();

    public EmployeeServiceTests(DatabaseFixture db)
    {
        _db = db;
        var lookup = Mock.Of<ICachedLookupService>();
        _service = new EmployeeService(db.DbContextFactory, lookup, Mock.Of<ILogger<EmployeeService>>(), ValidValidator.Create<EmployeeDto>(), ValidValidator.Create<StaffRoleDto>());
    }

    /// <summary>
    /// استرجاع النشط Async_ Returns الكل.
    /// </summary>
    [Fact]
    public async Task GetAllActiveAsync_ReturnsAll()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.Employees.Add(new Employee { FullName = "Emp1", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await ctx.SaveChangesAsync();

        var result = await _service.GetAllActiveAsync(CancellationToken.None);

        result.Should().Contain(e => e.FullName == "Emp1");
    }

    /// <summary>
    /// إنشاء Async_ Valid_ Returns Success.
    /// </summary>
    [Fact]
    public async Task CreateAsync_Valid_ReturnsSuccess()
    {
        var dto = new EmployeeDto(0, "New Emp", StaffRoleId: 1, null, null);

        var result = await _service.CreateAsync(dto, _admin, CancellationToken.None);

        result.ShouldBeSuccess();
    }

    /// <summary>
    /// استرجاع الكل الأدوار Async_ Returns الأدوار.
    /// </summary>
    [Fact]
    public async Task GetAllRolesAsync_ReturnsRoles()
    {
        var result = await _service.GetAllRolesAsync(CancellationToken.None);
        result.Should().Contain(r => r.RoleName == "مذيع");
    }

    /// <summary>
    /// إنشاء الدور Async_ Valid_ Returns Success.
    /// </summary>
    [Fact]
    public async Task CreateRoleAsync_Valid_ReturnsSuccess()
    {
        var dto = new StaffRoleDto(0, "مصور");

        var result = await _service.CreateRoleAsync(dto, _admin, CancellationToken.None);

        result.ShouldBeSuccess();
    }

    /// <summary>
    /// تحديث Async_ Valid_ Returns Success.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_Valid_ReturnsSuccess()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.Employees.Add(new Employee { EmployeeId = 30, FullName = "Old Name", StaffRoleId = 1, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await ctx.SaveChangesAsync();

        var dto = new EmployeeDto(30, "Updated Name", StaffRoleId: 1, null, null);
        var result = await _service.UpdateAsync(dto, _admin, CancellationToken.None);

        result.ShouldBeSuccess();
    }

    /// <summary>
    /// Soft Delete Async_ Valid_ Returns Success.
    /// </summary>
    [Fact]
    public async Task SoftDeleteAsync_Valid_ReturnsSuccess()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.Employees.Add(new Employee { EmployeeId = 40, FullName = "ToDelete", StaffRoleId = 1, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await ctx.SaveChangesAsync();

        var result = await _service.SoftDeleteAsync(40, _admin, CancellationToken.None);

        result.ShouldBeSuccess();
    }

    /// <summary>
    /// تحديث الدور Async_ Valid_ Returns Success.
    /// </summary>
    [Fact]
    public async Task UpdateRoleAsync_Valid_ReturnsSuccess()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.StaffRoles.Add(new StaffRole { StaffRoleId = 10, RoleName = "OldRole", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await ctx.SaveChangesAsync();

        var dto = new StaffRoleDto(10, "NewRole");
        var result = await _service.UpdateRoleAsync(dto, _admin, CancellationToken.None);

        result.ShouldBeSuccess();
    }

    /// <summary>
    /// Soft Delete الدور Async_ Valid_ Returns Success.
    /// </summary>
    [Fact]
    public async Task SoftDeleteRoleAsync_Valid_ReturnsSuccess()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.StaffRoles.Add(new StaffRole { StaffRoleId = 20, RoleName = "RoleToDelete", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await ctx.SaveChangesAsync();

        var result = await _service.SoftDeleteRoleAsync(20, _admin, CancellationToken.None);

        result.ShouldBeSuccess();
    }
}
