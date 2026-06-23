using DataAccess.Services;
using Domain.Models;
using Radio.Tests.TestData.Fixtures;

namespace Radio.Tests.Services;

public class PermissionServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _db;
    private readonly PermissionService _sut;

    public PermissionServiceTests(DatabaseFixture db)
    {
        _db = db;
        _sut = new PermissionService(db.DbContextFactory);
    }

    [Fact]
    public async Task GetAllPermissionsAsync_ReturnsAll()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.Permissions.Add(new Permission { PermissionId = 1, SystemName = "episode.view", DisplayName = "عرض الحلقات", Module = "Episodes" });
        ctx.Permissions.Add(new Permission { PermissionId = 2, SystemName = "guest.edit", DisplayName = "تعديل الضيوف", Module = "Guests" });
        await ctx.SaveChangesAsync();

        var result = await _sut.GetAllPermissionsAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain(p => p.SystemName == "episode.view");
        result.Value.Should().Contain(p => p.SystemName == "guest.edit");
    }

    [Fact]
    public async Task GetPermissionByIdAsync_Existing_ReturnsPermission()
    {
        await using var ctx = await _db.CreateContextAsync();
        ctx.Permissions.Add(new Permission { PermissionId = 10, SystemName = "episode.delete", DisplayName = "حذف الحلقات", Module = "Episodes" });
        await ctx.SaveChangesAsync();

        var result = await _sut.GetPermissionByIdAsync(10, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.SystemName.Should().Be("episode.delete");
    }

    [Fact]
    public async Task GetPermissionByIdAsync_NonExisting_ReturnsFail()
    {
        var result = await _sut.GetPermissionByIdAsync(999, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }
}
