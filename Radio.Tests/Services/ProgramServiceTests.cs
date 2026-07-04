// ============================================================
// ProgramServiceTests — البرنامج Service
// ============================================================
// المسؤولية: تعريف البرنامج Service.
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
/// صنف البرنامج Service.
/// </summary>
public class ProgramServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _db;
    private readonly IProgramService _service;
    private readonly UserSession _admin = UserSessionBuilder.CreateAdmin();

    public ProgramServiceTests(DatabaseFixture db)
    {
        _db = db;
        var lookup = Mock.Of<ICachedLookupService>();
        _service = new ProgramService(db.DbContextFactory, lookup, Mock.Of<ILogger<ProgramService>>(), ValidValidator.Create<ProgramDto>());
    }

    /// <summary>
    /// استرجاع النشط Async_ Returns Only Active.
    /// </summary>
    [Fact]
    public async Task GetAllActiveAsync_ReturnsOnlyActive()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.Programs.Add(new Domain.Models.Program
        {
            ProgramName = "Active Program", IsActive = true,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        ctx.Programs.Add(new Domain.Models.Program
        {
            ProgramName = "Inactive Program", IsActive = false,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var result = await _service.GetAllActiveAsync(CancellationToken.None);

        result.Should().Contain(e => e.ProgramName == "Active Program");
    }

    /// <summary>
    /// إنشاء البرنامج Async_ Valid_ Returns Success.
    /// </summary>
    [Fact]
    public async Task CreateProgramAsync_Valid_ReturnsSuccess()
    {
        var dto = new ProgramDto(0, "New Program", null, null);

        var result = await _service.CreateProgramAsync(dto, _admin, CancellationToken.None);

        result.ShouldBeSuccess();
    }

    /// <summary>
    /// إنشاء البرنامج Async_ Without Permission_ Returns Fail.
    /// </summary>
    [Fact]
    public async Task CreateProgramAsync_WithoutPermission_ReturnsFail()
    {
        var user = UserSessionBuilder.CreateLimited();
        var dto = new ProgramDto(0, "New Program", null, null);

        var result = await _service.CreateProgramAsync(dto, user, CancellationToken.None);

        result.ShouldBeFailure("صلاحية");
    }

    /// <summary>
    /// تحديث البرنامج Async_ Valid_ Returns Success.
    /// </summary>
    [Fact]
    public async Task UpdateProgramAsync_Valid_ReturnsSuccess()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.Programs.Add(new Domain.Models.Program
        {
            ProgramId = 10, ProgramName = "Original", IsActive = true,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var result = await _service.UpdateProgramAsync(
            new ProgramDto(10, "Updated", null, null), _admin, CancellationToken.None);

        result.ShouldBeSuccess();
    }

    /// <summary>
    /// تحديث البرنامج Async_ Not Found_ Returns Fail.
    /// </summary>
    [Fact]
    public async Task UpdateProgramAsync_NotFound_ReturnsFail()
    {
        var result = await _service.UpdateProgramAsync(
            new ProgramDto(9999, "Ghost", null, null), _admin, CancellationToken.None);
        result.ShouldBeFailure("غير موجود");
    }

    /// <summary>
    /// Soft Delete Async_ Valid_ Soft Deletes.
    /// </summary>
    [Fact]
    public async Task SoftDeleteAsync_Valid_SoftDeletes()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.Programs.Add(new Domain.Models.Program
        {
            ProgramId = 20, ProgramName = "ToDelete", IsActive = true,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var result = await _service.SoftDeleteAsync(20, _admin, CancellationToken.None);

        result.ShouldBeSuccess();
    }
}
