using DataAccess.Common;
using DataAccess.DTOs;
using DataAccess.Services;
using Domain.Models;
using Radio.Tests.Helpers;
using Radio.Tests.TestData.Builders;
using Radio.Tests.TestData.Fixtures;

namespace Radio.Tests.Services;

public class CoverageServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _db;
    private readonly ICoverageService _service;
    private readonly UserSession _admin = UserSessionBuilder.CreateAdmin();

    public CoverageServiceTests(DatabaseFixture db)
    {
        _db = db;
        _service = new CoverageService(db.DbContextFactory);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsCoverages()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.CorrespondentCoverages.Add(new CorrespondentCoverage
        {
            CorrespondentId = 1, Location = "City Center", IsActive = true,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var result = await _service.GetAllAsync();
        result.Should().Contain(c => c.Location == "City Center");
    }

    [Fact]
    public async Task DeleteAsync_Valid_SoftDeletes()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.CorrespondentCoverages.Add(new CorrespondentCoverage
        {
            CoverageId = 50, CorrespondentId = 1, Location = "Loc", IsActive = true,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var result = await _service.DeleteAsync(50, _admin);
        result.ShouldBeSuccess();
    }
}
