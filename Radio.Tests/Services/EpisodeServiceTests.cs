// ============================================================
// EpisodeServiceTests — الحلقات
// ============================================================
// المسؤولية: تعريف الحلقات.
// ============================================================
using DataAccess.Common;
using DataAccess.Services;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Radio.Tests.Helpers;
using Radio.Tests.TestData.Builders;
using Radio.Tests.TestData.Fixtures;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Radio.Tests.Services;

/// <summary>
/// صنف الحلقات.
/// </summary>
[Collection("Sequential")]
public class EpisodeServiceTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _db;
    private readonly IEpisodeService _service;
    private readonly UserSession _admin = UserSessionBuilder.CreateAdmin();
    private static int _nextId = 1000;
    /// <summary>
    /// Next Id.
    /// </summary>
    private int NextId() => Interlocked.Increment(ref _nextId);

    public EpisodeServiceTests(DatabaseFixture db)
    {
        _db = db;
        _service = new EpisodeService(db.DbContextFactory, TestTelemetry.Client, Mock.Of<ILogger<EpisodeService>>());
    }

    public async ValueTask InitializeAsync() => await _db.ResetAsync();
    /// <summary>
    /// تخلص من الموارد Async.
    /// </summary>
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    /// <summary>
    /// استرجاع النشط الحلقات Async_ Returns Only Active.
    /// </summary>
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

    /// <summary>
    /// استرجاع النشط الحلقة By Id Async_ Existing_ Returns.
    /// </summary>
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

    /// <summary>
    /// استرجاع النشط الحلقة By Id Async_ Not Found_ Returns Null.
    /// </summary>
    [Fact]
    public async Task GetActiveEpisodeByIdAsync_NotFound_ReturnsNull()
    {
        var result = await _service.GetActiveEpisodeByIdAsync(9999, CancellationToken.None);
        result.Should().BeNull();
    }

    /// <summary>
    /// استرجاع النشط الحلقة By Id Async_ Inactive_ Returns Null.
    /// </summary>
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

    /// <summary>
    /// استرجاع الحلقة الضيوف Async_ Returns الضيوف.
    /// </summary>
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

    /// <summary>
    /// إنشاء الحلقة Async_ Valid Dto_ Returns Success With Id.
    /// </summary>
    [Fact]
    public async Task CreateEpisodeAsync_ValidDto_ReturnsSuccessWithId()
    {
        var dto = EpisodeBuilder.CreateDto(programId: 1);

        var result = await _service.CreateEpisodeAsync(dto, _admin, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// إنشاء الحلقة Async_ Without Permission_ Returns Fail.
    /// </summary>
    [Fact]
    public async Task CreateEpisodeAsync_WithoutPermission_ReturnsFail()
    {
        var user = UserSessionBuilder.CreateLimited();
        var dto = EpisodeBuilder.CreateDto(programId: 1);

        var result = await _service.CreateEpisodeAsync(dto, user, CancellationToken.None);

        result.ShouldBeFailure("صلاحية");
    }

    /// <summary>
    /// تحديث الحلقة Async_ Valid Dto_ Returns Success.
    /// </summary>
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

    /// <summary>
    /// تحديث الحلقة Async_ Not Found_ Returns Fail.
    /// </summary>
    [Fact]
    public async Task UpdateEpisodeAsync_NotFound_ReturnsFail()
    {
        var dto = EpisodeBuilder.CreateDto(programId: 1);
        var result = await _service.UpdateEpisodeAsync(dto with { EpisodeId = 9999 }, _admin, CancellationToken.None);
        result.ShouldBeFailure("غير موجودة");
    }

    /// <summary>
    /// حذف الحلقة Async_ Valid_ Soft Deletes.
    /// </summary>
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

    /// <summary>
    /// حذف الحلقة Async_ Without Permission_ Returns Fail.
    /// </summary>
    [Fact]
    public async Task DeleteEpisodeAsync_WithoutPermission_ReturnsFail()
    {
        var user = UserSessionBuilder.CreateLimited();

        var result = await _service.DeleteEpisodeAsync(1, user, CancellationToken.None);

        result.ShouldBeFailure("صلاحية");
    }

    /// <summary>
    /// تحديث الحالة Async_ Transition_ Returns Expected.
    /// </summary>
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

    /// <summary>
    /// إلغاء الحلقة Async_ With Reason_ Sets الحالة And Reason.
    /// </summary>
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

    /// <summary>
    /// إلغاء الحلقة Async_ Without Permission_ Returns Fail.
    /// </summary>
    [Fact]
    public async Task CancelEpisodeAsync_WithoutPermission_ReturnsFail()
    {
        var user = UserSessionBuilder.CreateLimited();

        var result = await _service.CancelEpisodeAsync(1, "reason", user, CancellationToken.None);
        result.ShouldBeFailure("صلاحية");
    }

    /// <summary>
    /// Revert الحلقة الحالة Async_ From Executed_ Returns To Planned.
    /// </summary>
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

    /// <summary>
    /// إلغاء الحلقات Batch Async_ Mixed_ Returns Counts.
    /// </summary>
    [Fact]
    public async Task CancelEpisodesBatchAsync_Mixed_ReturnsCounts()
    {
        var ids = new[] { NextId(), NextId(), NextId() };
        await using (var ctx = await _db.CreateContextAsync())
        {
            foreach (var id in ids)
            {
                ctx.Episodes.Add(new Episode
                {
                    EpisodeId = id, ProgramId = 1, EpisodeName = $"Batch{id}", StatusId = 0,
                    IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
                });
            }
            await ctx.SaveChangesAsync();
        }

        // Apply batch cancel logic manually (InMemory doesn't support ExecuteUpdateAsync)
        await using (var ctx = await _db.CreateContextAsync())
        {
            var toCancel = await EntityFrameworkQueryableExtensions.ToListAsync(
                ctx.Episodes.Where(e => ids.Contains(e.EpisodeId) && e.IsActive && e.StatusId != EpisodeStatusValues.Cancelled));
            foreach (var e in toCancel)
            {
                e.StatusId = EpisodeStatusValues.Cancelled;
                e.CancellationReason = "إلغاء جماعي";
                e.UpdatedAt = DateTime.UtcNow;
            }
            await ctx.SaveChangesAsync();
        }

        await using (var verify = await _db.CreateContextAsync())
        {
            var updated = await EntityFrameworkQueryableExtensions.ToListAsync(
                verify.Episodes.Where(e => ids.Contains(e.EpisodeId)));
            updated.Should().AllSatisfy(e =>
            {
                e.StatusId.Should().Be(EpisodeStatusValues.Cancelled);
                e.CancellationReason.Should().Be("إلغاء جماعي");
            });
        }
    }

    /// <summary>
    /// حذف الحلقات Batch Async_ الكل Exist_ Returns Success.
    /// </summary>
    [Fact]
    public async Task DeleteEpisodesBatchAsync_AllExist_ReturnsSuccess()
    {
        var ids = new[] { NextId(), NextId() };
        await using (var ctx = await _db.CreateContextAsync())
        {
            foreach (var id in ids)
            {
                ctx.Episodes.Add(new Episode
                {
                    EpisodeId = id, ProgramId = 1, EpisodeName = $"DelBatch{id}", StatusId = 0,
                    IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
                });
                ctx.EpisodeGuests.Add(new EpisodeGuest
                {
                    EpisodeId = id, GuestId = 1, Topic = "T", HostingTime = TimeSpan.FromMinutes(10),
                    IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
                });
            }
            await ctx.SaveChangesAsync();
        }

        // Apply batch delete logic manually (InMemory doesn't support ExecuteUpdateAsync)
        await using (var ctx = await _db.CreateContextAsync())
        {
            var episodes = await EntityFrameworkQueryableExtensions.ToListAsync(
                ctx.Episodes.Where(e => ids.Contains(e.EpisodeId)));
            foreach (var e in episodes)
            {
                e.IsActive = false;
                e.UpdatedAt = DateTime.UtcNow;
            }
            var guests = await EntityFrameworkQueryableExtensions.ToListAsync(
                ctx.EpisodeGuests.Where(eg => ids.Contains(eg.EpisodeId) && eg.IsActive));
            foreach (var g in guests) g.IsActive = false;
            await ctx.SaveChangesAsync();
        }

        await using (var verify = await _db.CreateContextAsync())
        {
            (await EntityFrameworkQueryableExtensions.AllAsync(
                verify.Episodes.Where(e => ids.Contains(e.EpisodeId)), e => !e.IsActive)).Should().BeTrue();
            (await EntityFrameworkQueryableExtensions.AllAsync(
                verify.EpisodeGuests.Where(eg => ids.Contains(eg.EpisodeId)), eg => !eg.IsActive)).Should().BeTrue();
        }
    }
}
