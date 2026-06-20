using DataAccess.Common;
using DataAccess.DTOs;
using DataAccess.Services;
using Domain.Models;
using Radio.Tests.Helpers;
using Radio.Tests.TestData.Builders;
using Radio.Tests.TestData.Fixtures;
using Microsoft.Extensions.Logging;

namespace Radio.Tests.Services;

public class PlatformServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _db;
    private readonly IPlatformService _service;
    private readonly UserSession _admin = UserSessionBuilder.CreateAdmin();

    public PlatformServiceTests(DatabaseFixture db)
    {
        _db = db;
        var lookup = Mock.Of<ICachedLookupService>();
        _service = new PlatformService(db.DbContextFactory, lookup, Mock.Of<ILogger<PlatformService>>());
    }

    [Fact]
    public async Task GetAllActiveAsync_ReturnsAll()
    {
        var result = await _service.GetAllActiveAsync();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task CreateAsync_Valid_ReturnsSuccess()
    {
        var dto = new SocialMediaPlatformDto(0, "YouTube", "youtube");
        var result = await _service.CreateAsync(dto, _admin);
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task UpdateAsync_Valid_ReturnsSuccess()
    {
        var dto = new SocialMediaPlatformDto(1, "Facebook Updated", "facebook");
        var result = await _service.UpdateAsync(dto, _admin);
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task DeleteAsync_Valid_SoftDeletes()
    {
        var result = await _service.DeleteAsync(1, _admin);
        result.ShouldBeSuccess();
    }
}
