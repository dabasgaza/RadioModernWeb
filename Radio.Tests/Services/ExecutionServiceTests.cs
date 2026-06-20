using DataAccess.Common;
using DataAccess.DTOs;
using DataAccess.Services;
using Domain.Models;
using Radio.Tests.Helpers;
using Radio.Tests.TestData.Builders;
using Radio.Tests.TestData.Fixtures;

namespace Radio.Tests.Services;

public class ExecutionServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _db;
    private readonly IExecutionService _service;
    private readonly UserSession _admin = UserSessionBuilder.CreateAdmin();

    public ExecutionServiceTests(DatabaseFixture db)
    {
        _db = db;
        _service = new ExecutionService(db.DbContextFactory);
    }

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

        var result = await _service.LogExecutionAsync(dto, _admin);
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task LogExecutionAsync_WithoutPermission_ReturnsFail()
    {
        var user = UserSessionBuilder.CreateLimited();

        var dto = new ExecutionLogDto
        {
            EpisodeId = 1, ExecutedByUserId = 1,
            ExecutionNotes = "Done", IssuesEncountered = "None"
        };

        var result = await _service.LogExecutionAsync(dto, user);
        result.ShouldBeFailure("صلاحية");
    }

    [Fact]
    public async Task LogExecutionAsync_EpisodeNotFound_ReturnsFail()
    {
        var dto = new ExecutionLogDto
        {
            EpisodeId = 9999, ExecutedByUserId = 1,
            ExecutionNotes = "Done", IssuesEncountered = "None"
        };

        var result = await _service.LogExecutionAsync(dto, _admin);
        result.ShouldBeFailure("غير موجودة");
    }
}
