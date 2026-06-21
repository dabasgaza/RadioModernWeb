using DataAccess.Common;
using DataAccess.DTOs;
using DataAccess.Services;
using Domain.Models;
using Radio.Tests.Helpers;
using Radio.Tests.TestData.Builders;
using Radio.Tests.TestData.Fixtures;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Radio.Tests.Services;

public class CorrespondentServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _db;
    private readonly ICorrespondentService _service;
    private readonly UserSession _admin = UserSessionBuilder.CreateAdmin();

    public CorrespondentServiceTests(DatabaseFixture db)
    {
        _db = db;
        var lookup = Mock.Of<ICachedLookupService>();
        _service = new CorrespondentService(db.DbContextFactory, lookup, Mock.Of<ILogger<CorrespondentService>>());
    }

    [Fact]
    public async Task GetAllActiveAsync_ReturnsOnlyActive()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.Correspondents.Add(new Correspondent { FullName = "Active Corr", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        ctx.Correspondents.Add(new Correspondent { FullName = "Inactive Corr", IsActive = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await ctx.SaveChangesAsync();

        var result = await _service.GetAllActiveAsync(CancellationToken.None);

        result.Should().Contain(e => e.FullName == "Active Corr");
    }

    [Fact]
    public async Task CreateAsync_Valid_ReturnsSuccess()
    {
        var dto = new CorrespondentDto(0, "New Corr", "123456789", null);
        var result = await _service.CreateAsync(dto, _admin, CancellationToken.None);
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task UpdateAsync_Valid_ReturnsSuccess()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.Correspondents.Add(new Correspondent { CorrespondentId = 10, FullName = "Original", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await ctx.SaveChangesAsync();

        var dto = new CorrespondentDto(10, "Updated", "987654321", null);
        var result = await _service.UpdateAsync(dto, _admin, CancellationToken.None);
        result.ShouldBeSuccess();
    }
}
