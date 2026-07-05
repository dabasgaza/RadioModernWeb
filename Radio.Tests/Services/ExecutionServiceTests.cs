// ============================================================
// ExecutionServiceTests — Execution Service
// ============================================================
// المسؤولية: تعريف Execution Service.
// ============================================================
using DataAccess.Common;
using DataAccess.DTOs;
using DataAccess.Services;
using Domain.Models;
using Radio.Tests.Helpers;
using Radio.Tests.TestData.Builders;
using Radio.Tests.TestData.Fixtures;
using System.Threading;

namespace Radio.Tests.Services;

/// <summary>
/// صنف Execution Service.
/// </summary>
public class ExecutionServiceTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _db;
    private readonly IExecutionService _service;
    private readonly UserSession _admin = UserSessionBuilder.CreateAdmin();

    public ExecutionServiceTests(DatabaseFixture db)
    {
        _db = db;
        _service = new ExecutionService(db.DbContextFactory);
    }

    public async ValueTask InitializeAsync() => await _db.ResetAsync();
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    /// <summary>
    /// تسجيل Execution Async_ Valid_ Returns Success.
    /// </summary>
    [Fact]
    public async Task LogExecutionAsync_Valid_ReturnsSuccess()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.Episodes.Add(new Episode
        {
            EpisodeId = 1, ProgramId = 1, EpisodeName = "ExecTest", StatusId = 0,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var dto = new ExecutionLogDto
        {
            EpisodeId = 1, ExecutedByUserId = 1,
            ExecutionNotes = "Done", IssuesEncountered = "None",
            DurationMinutes = 45
        };

        var result = await _service.LogExecutionAsync(dto, _admin, CancellationToken.None);
        result.ShouldBeSuccess();
    }

    /// <summary>
    /// تسجيل Execution Async_ Without Permission_ Returns Fail.
    /// </summary>
    [Fact]
    public async Task LogExecutionAsync_WithoutPermission_ReturnsFail()
    {
        var user = UserSessionBuilder.CreateLimited();

        var dto = new ExecutionLogDto
        {
            EpisodeId = 1, ExecutedByUserId = 1,
            ExecutionNotes = "Done", IssuesEncountered = "None"
        };

        var result = await _service.LogExecutionAsync(dto, user, CancellationToken.None);
        result.ShouldBeFailure("صلاحية");
    }

    /// <summary>
    /// تسجيل Execution Async_ الحلقة Not Found_ Returns Fail.
    /// </summary>
    [Fact]
    public async Task LogExecutionAsync_EpisodeNotFound_ReturnsFail()
    {
        var dto = new ExecutionLogDto
        {
            EpisodeId = 9999, ExecutedByUserId = 1,
            ExecutionNotes = "Done", IssuesEncountered = "None"
        };

        var result = await _service.LogExecutionAsync(dto, _admin, CancellationToken.None);
        result.ShouldBeFailure("غير موجودة");
    }

    /// <summary>
    /// تسجيل Execution Async_ Already Published_ Returns Fail.
    /// </summary>
    [Fact]
    public async Task LogExecutionAsync_AlreadyPublished_ReturnsFail()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.Episodes.Add(new Episode
        {
            EpisodeId = 2, ProgramId = 1, EpisodeName = "Published", StatusId = EpisodeStatusValues.Published,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var dto = new ExecutionLogDto { EpisodeId = 2, ExecutedByUserId = 1 };

        var result = await _service.LogExecutionAsync(dto, _admin, CancellationToken.None);
        result.ShouldBeFailure("نشر");
    }

    /// <summary>
    /// استرجاع Execution سجل Async_ Existing_ Returns سجل.
    /// </summary>
    [Fact]
    public async Task GetExecutionLogAsync_Existing_ReturnsLog()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.Episodes.Add(new Episode
        {
            EpisodeId = 10, ProgramId = 1, EpisodeName = "GetLog", StatusId = EpisodeStatusValues.Executed,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        ctx.ExecutionLogs.Add(new ExecutionLog
        {
            ExecutionLogId = 1, EpisodeId = 10, ExecutedByUserId = 1,
            DurationMinutes = 30, IsActive = true, CreatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var result = await _service.GetExecutionLogAsync(10, CancellationToken.None);

        result.Should().NotBeNull();
        result!.EpisodeId.Should().Be(10);
    }

    /// <summary>
    /// استرجاع Execution سجل Async_ Non Existing_ Returns Null.
    /// </summary>
    [Fact]
    public async Task GetExecutionLogAsync_NonExisting_ReturnsNull()
    {
        var result = await _service.GetExecutionLogAsync(999, CancellationToken.None);

        result.Should().BeNull();
    }

    /// <summary>
    /// تحديث Execution سجل Async_ Valid_ Returns Success.
    /// </summary>
    [Fact]
    public async Task UpdateExecutionLogAsync_Valid_ReturnsSuccess()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.Episodes.Add(new Episode
        {
            EpisodeId = 20, ProgramId = 1, EpisodeName = "UpdLog", StatusId = EpisodeStatusValues.Executed,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        ctx.ExecutionLogs.Add(new ExecutionLog
        {
            ExecutionLogId = 2, EpisodeId = 20, ExecutedByUserId = 1,
            DurationMinutes = 30, IsActive = true, CreatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var dto = new ExecutionLogDto
        {
            ExecutionLogId = 2, EpisodeId = 20, DurationMinutes = 45,
            ExecutionNotes = "Updated notes"
        };
        var result = await _service.UpdateExecutionLogAsync(dto, _admin, CancellationToken.None);

        result.ShouldBeSuccess();
    }

    /// <summary>
    /// تحديث Execution سجل Async_ Non Existing_ Returns Fail.
    /// </summary>
    [Fact]
    public async Task UpdateExecutionLogAsync_NonExisting_ReturnsFail()
    {
        var dto = new ExecutionLogDto { ExecutionLogId = 999, EpisodeId = 1 };

        var result = await _service.UpdateExecutionLogAsync(dto, _admin, CancellationToken.None);
        result.ShouldBeFailure("غير موجود");
    }

    /// <summary>
    /// تحديث Execution سجل Async_ Without Permission_ Returns Fail.
    /// </summary>
    [Fact]
    public async Task UpdateExecutionLogAsync_WithoutPermission_ReturnsFail()
    {
        var user = UserSessionBuilder.CreateLimited();

        var dto = new ExecutionLogDto { ExecutionLogId = 1, EpisodeId = 1 };

        var result = await _service.UpdateExecutionLogAsync(dto, user, CancellationToken.None);
        result.ShouldBeFailure("صلاحية");
    }
}
