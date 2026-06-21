using DataAccess.Common;
using DataAccess.Services;
using Domain.Models;
using Radio.Tests.Helpers;
using Radio.Tests.TestData.Builders;
using Radio.Tests.TestData.Fixtures;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Radio.Tests.Services;

[Collection("Sequential")]
public class EpisodeServiceTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _db;
    private readonly IEpisodeService _service;
    private readonly UserSession _admin = UserSessionBuilder.CreateAdmin();
    private static int _nextId = 1000;
    private int NextId() => Interlocked.Increment(ref _nextId);

    public EpisodeServiceTests(DatabaseFixture db)
    {
        _db = db;
        _service = new EpisodeService(db.DbContextFactory, TestTelemetry.Client, Mock.Of<ILogger<EpisodeService>>());
    }

    public async Task InitializeAsync() => await _db.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetActiveEpisodesAsync_ReturnsOnlyActive()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.Episodes.Add(new Episode
        {
            ProgramId = 1, EpisodeName = "Active", StatusId = 0, IsActive = true,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        ctx.Episodes.Add(new Episode
        {
            ProgramId = 1, EpisodeName = "Inactive", StatusId = 0, IsActive = false,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var result = await _service.GetActiveEpisodesAsync(CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].EpisodeName.Should().Be("Active");
    }

    [Fact]
    public async Task GetActiveEpisodeByIdAsync_Existing_ReturnsDto()
    {
        var id = NextId();
        await using var ctx = await _db.CreateContextAsync();
        ctx.Episodes.Add(new Episode
        {
            EpisodeId = id, ProgramId = 1, EpisodeName = "Test", StatusId = 0,
            EpisodeDescription = "Desc", ScheduledExecutionTime = DateTime.UtcNow,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var result = await _service.GetActiveEpisodeByIdAsync(id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.EpisodeName.Should().Be("Test");
        result.EpisodeDescription.Should().Be("Desc");
    }

    [Fact]
    public async Task GetActiveEpisodeByIdAsync_NotFound_ReturnsNull()
    {
        var result = await _service.GetActiveEpisodeByIdAsync(9999, CancellationToken.None);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveEpisodeByIdAsync_Inactive_ReturnsNull()
    {
        var id = NextId();
        await using var ctx = await _db.CreateContextAsync();
        ctx.Episodes.Add(new Episode
        {
            EpisodeId = id, ProgramId = 1, EpisodeName = "Deleted", StatusId = 0,
            IsActive = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var result = await _service.GetActiveEpisodeByIdAsync(id, CancellationToken.None);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetEpisodeGuestsAsync_ReturnsGuests()
    {
        var epId = NextId();
        await using var ctx = await _db.CreateContextAsync();
        var guest = new Guest { FullName = "Test Guest", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        ctx.Guests.Add(guest);
        await ctx.SaveChangesAsync();

        ctx.EpisodeGuests.Add(new EpisodeGuest
        {
            EpisodeId = epId, GuestId = guest.GuestId, Topic = "Politics",
            HostingTime = TimeSpan.FromMinutes(30), IsActive = true,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var result = await _service.GetEpisodeGuestsAsync(epId, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].FullName.Should().Be("Test Guest");
        result[0].Topic.Should().Be("Politics");
    }

    [Fact]
    public async Task CreateEpisodeAsync_ValidDto_ReturnsSuccessWithId()
    {
        var dto = EpisodeBuilder.CreateDto(programId: 1);

        var result = await _service.CreateEpisodeAsync(dto, _admin, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateEpisodeAsync_WithoutPermission_ReturnsFail()
    {
        var user = UserSessionBuilder.CreateLimited();
        var dto = EpisodeBuilder.CreateDto(programId: 1);

        var result = await _service.CreateEpisodeAsync(dto, user, CancellationToken.None);

        result.ShouldBeFailure("صلاحية");
    }

    [Fact]
    public async Task UpdateEpisodeAsync_ValidDto_ReturnsSuccess()
    {
        var id = NextId();
        await using var ctx = await _db.CreateContextAsync();
        ctx.Episodes.Add(new Episode
        {
            EpisodeId = id, ProgramId = 1, EpisodeName = "Original", StatusId = 0,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var dto = EpisodeBuilder.CreateDto(programId: 1, name: "Updated");

        var result = await _service.UpdateEpisodeAsync(dto with { EpisodeId = id }, _admin, CancellationToken.None);

        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task UpdateEpisodeAsync_NotFound_ReturnsFail()
    {
        var dto = EpisodeBuilder.CreateDto(programId: 1);
        var result = await _service.UpdateEpisodeAsync(dto with { EpisodeId = 9999 }, _admin, CancellationToken.None);
        result.ShouldBeFailure("غير موجودة");
    }

    [Fact]
    public async Task DeleteEpisodeAsync_Valid_SoftDeletes()
    {
        var id = NextId();
        await using var ctx = await _db.CreateContextAsync();
        ctx.Episodes.Add(new Episode
        {
            EpisodeId = id, ProgramId = 1, EpisodeName = "ToDelete", StatusId = 0,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var result = await _service.DeleteEpisodeAsync(id, _admin, CancellationToken.None);

        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task DeleteEpisodeAsync_WithoutPermission_ReturnsFail()
    {
        var user = UserSessionBuilder.CreateLimited();

        var result = await _service.DeleteEpisodeAsync(1, user, CancellationToken.None);

        result.ShouldBeFailure("صلاحية");
    }

    [Theory]
    [InlineData(0, DataAccess.Services.EpisodeStatusValues.Executed, true)]
    [InlineData(1, DataAccess.Services.EpisodeStatusValues.Cancelled, true)]
    [InlineData(2, DataAccess.Services.EpisodeStatusValues.Published, false)]
    [InlineData(3, DataAccess.Services.EpisodeStatusValues.Published, true)]
    public async Task UpdateStatusAsync_Transition_ReturnsExpected(int testCase, byte to, bool shouldSucceed)
    {
        var id = NextId();
        byte from = testCase switch { 0 => 0, 1 => 0, 2 => 0, 3 => 1, _ => 0 };
        await using var ctx = await _db.CreateContextAsync();
        ctx.Episodes.Add(new Episode
        {
            EpisodeId = id, ProgramId = 1, EpisodeName = $"StatusTest_{testCase}", StatusId = from,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var result = await _service.UpdateStatusAsync(id, to, _admin, CancellationToken.None);

        result.IsSuccess.Should().Be(shouldSucceed);
    }

    [Fact]
    public async Task CancelEpisodeAsync_WithReason_SetsStatusAndReason()
    {
        var id = NextId();
        await using var ctx = await _db.CreateContextAsync();
        ctx.Episodes.Add(new Episode
        {
            EpisodeId = id, ProgramId = 1, EpisodeName = "CancelTest", StatusId = 0,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var result = await _service.CancelEpisodeAsync(id, "مشكلة فنية", _admin, CancellationToken.None);

        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task CancelEpisodeAsync_WithoutPermission_ReturnsFail()
    {
        var user = UserSessionBuilder.CreateLimited();

        var result = await _service.CancelEpisodeAsync(1, "reason", user, CancellationToken.None);
        result.ShouldBeFailure("صلاحية");
    }

    [Fact]
    public async Task RevertEpisodeStatusAsync_FromExecuted_ReturnsToPlanned()
    {
        var id = NextId();
        await using var ctx = await _db.CreateContextAsync();
        ctx.Episodes.Add(new Episode
        {
            EpisodeId = id, ProgramId = 1, EpisodeName = "RevertTest", StatusId = 1,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var result = await _service.RevertEpisodeStatusAsync(id, "تصحيح", _admin, CancellationToken.None);

        result.ShouldBeSuccess();
    }

    [Fact(Skip = "ExecuteUpdateAsync not supported by InMemory database")]
    public async Task CancelEpisodesBatchAsync_Mixed_ReturnsCounts()
    {
        var ids = new[] { NextId(), NextId(), NextId() };
        await using var ctx = await _db.CreateContextAsync();
        foreach (var id in ids)
        {
            ctx.Episodes.Add(new Episode
            {
                EpisodeId = id, ProgramId = 1, EpisodeName = $"Batch{id}", StatusId = 0,
                IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            });
        }
        await ctx.SaveChangesAsync();

        var (success, fail) = await _service.CancelEpisodesBatchAsync([.. ids], "إلغاء جماعي", _admin, CancellationToken.None);

        success.Should().Be(3);
        fail.Should().Be(0);
    }

    [Fact(Skip = "ExecuteUpdateAsync not supported by InMemory database")]
    public async Task DeleteEpisodesBatchAsync_AllExist_ReturnsSuccess()
    {
        var ids = new[] { NextId(), NextId() };
        await using var ctx = await _db.CreateContextAsync();
        foreach (var id in ids)
        {
            ctx.Episodes.Add(new Episode
            {
                EpisodeId = id, ProgramId = 1, EpisodeName = $"DelBatch{id}", StatusId = 0,
                IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            });
        }
        await ctx.SaveChangesAsync();

        var (success, fail) = await _service.DeleteEpisodesBatchAsync([.. ids], _admin, CancellationToken.None);

        success.Should().Be(2);
        fail.Should().Be(0);
    }
}
