// ============================================================
// PlatformServiceTests — Platform Service
// ============================================================
// المسؤولية: تعريف Platform Service.
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
/// صنف Platform Service.
/// </summary>
public class PlatformServiceTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _db;
    private readonly IPlatformService _service;
    private readonly UserSession _admin = UserSessionBuilder.CreateAdmin();

    public PlatformServiceTests(DatabaseFixture db)
    {
        _db = db;
        var lookup = Mock.Of<ICachedLookupService>();
        _service = new PlatformService(db.DbContextFactory, lookup, Mock.Of<ILogger<PlatformService>>(), ValidValidator.Create<SocialMediaPlatformDto>());
    }

    public async ValueTask InitializeAsync() => await _db.ResetAsync();
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    /// <summary>
    /// استرجاع النشط Async_ Returns الكل.
    /// </summary>
    [Fact]
    public async Task GetAllActiveAsync_ReturnsAll()
    {
        var result = await _service.GetAllActiveAsync(CancellationToken.None);
        result.Should().HaveCount(3);
    }

    /// <summary>
    /// إنشاء Async_ Valid_ Returns Success.
    /// </summary>
    [Fact]
    public async Task CreateAsync_Valid_ReturnsSuccess()
    {
        var dto = new SocialMediaPlatformDto(0, "YouTube", "youtube");
        var result = await _service.CreateAsync(dto, _admin, CancellationToken.None);
        result.ShouldBeSuccess();
    }

    /// <summary>
    /// تحديث Async_ Valid_ Returns Success.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_Valid_ReturnsSuccess()
    {
        var dto = new SocialMediaPlatformDto(1, "Facebook Updated", "facebook");
        var result = await _service.UpdateAsync(dto, _admin, CancellationToken.None);
        result.ShouldBeSuccess();
    }

    /// <summary>
    /// حذف Async_ Valid_ Soft Deletes.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_Valid_SoftDeletes()
    {
        var result = await _service.DeleteAsync(1, _admin, CancellationToken.None);
        result.ShouldBeSuccess();
    }
}
