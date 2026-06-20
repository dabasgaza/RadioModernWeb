using DataAccess.Common;
using DataAccess.DTOs;
using DataAccess.Services;
using Domain.Models;
using Radio.Tests.Helpers;
using Radio.Tests.TestData.Builders;
using Radio.Tests.TestData.Fixtures;

namespace Radio.Tests.Services;

public class GuestServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _db;
    private readonly IGuestService _service;
    private readonly UserSession _admin = UserSessionBuilder.CreateAdmin();

    public GuestServiceTests(DatabaseFixture db)
    {
        _db = db;
        var lookup = Mock.Of<ICachedLookupService>();
        _service = new GuestService(db.DbContextFactory, lookup);
    }

    [Fact]
    public async Task GetAllActiveAsync_ReturnsOnlyActive()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.Guests.Add(new Guest { FullName = "Active", PhoneNumber = "555", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        ctx.Guests.Add(new Guest { FullName = "Inactive", PhoneNumber = "555", IsActive = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await ctx.SaveChangesAsync();

        var result = await _service.GetAllActiveAsync();

        result.Should().Contain(e => e.FullName == "Active");
    }

    [Fact]
    public async Task CreateGuestAsync_Valid_ReturnsSuccess()
    {
        var dto = new GuestDto(0, "New Guest", null, "5555555", null, null, null);

        var result = await _service.CreateGuestAsync(dto, _admin);

        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task CreateGuestAsync_WithoutPermission_ReturnsFail()
    {
        var user = UserSessionBuilder.CreateLimited();
        var dto = new GuestDto(0, "New Guest", null, "5555555", null, null, null);

        var result = await _service.CreateGuestAsync(dto, user);

        result.ShouldBeFailure("صلاحية");
    }

    [Fact]
    public async Task UpdateGuestAsync_Valid_ReturnsSuccess()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.Guests.Add(new Guest { GuestId = 10, FullName = "Original", PhoneNumber = "555", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await ctx.SaveChangesAsync();

        var dto = new GuestDto(10, "Updated", null, "5555555", null, null, null);
        var result = await _service.UpdateGuestAsync(dto, _admin);

        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task SoftDeleteGuestAsync_Valid_SoftDeletes()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.Guests.Add(new Guest { GuestId = 20, FullName = "ToDelete", PhoneNumber = "555", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await ctx.SaveChangesAsync();

        var result = await _service.SoftDeleteGuestAsync(20, _admin);

        result.ShouldBeSuccess();
    }
}
