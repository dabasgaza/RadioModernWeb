using DataAccess.Common;
using DataAccess.DTOs;
using DataAccess.Services;
using Domain.Models;
using Radio.Tests.Helpers;
using Radio.Tests.TestData.Builders;
using Radio.Tests.TestData.Fixtures;
using System.Threading;

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

        var result = await _service.GetAllAsync(CancellationToken.None);
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

        var result = await _service.DeleteAsync(50, _admin, CancellationToken.None);
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task CreateAsync_Valid_ReturnsSuccess()
    {
        var dto = new CoverageDto { CorrespondentId = 1, Topic = "تغطية جديدة", Location = "مكان" };

        var result = await _service.CreateAsync(dto, _admin, CancellationToken.None);

        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsCoverage()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.CorrespondentCoverages.Add(new CorrespondentCoverage
        {
            CoverageId = 60, CorrespondentId = 1, Location = "FindMe", Topic = "موضوع", IsActive = true,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var result = await _service.GetByIdAsync(60, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Location.Should().Be("FindMe");
    }

    [Fact]
    public async Task GetByIdAsync_NonExisting_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(999, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_Valid_ReturnsSuccess()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.CorrespondentCoverages.Add(new CorrespondentCoverage
        {
            CoverageId = 70, CorrespondentId = 1, Location = "Old", Topic = "قديم", IsActive = true,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var dto = new CoverageDto { CoverageId = 70, CorrespondentId = 1, Location = "New", Topic = "جديد" };

        var result = await _service.UpdateAsync(dto, _admin, CancellationToken.None);
        result.ShouldBeSuccess();
    }
}
