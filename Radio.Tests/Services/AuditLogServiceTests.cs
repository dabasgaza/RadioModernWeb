using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataAccess.Services;
using Domain.Identity;
using Domain.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Radio.Tests.TestData.Fixtures;
using Xunit;

namespace Radio.Tests.Services
{
    public class AuditLogServiceTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _db;
        private readonly AuditLogService _sut;

        public AuditLogServiceTests(DatabaseFixture db)
        {
            _db = db;
            _sut = new AuditLogService(db.DbContextFactory, Mock.Of<ILogger<AuditLogService>>());
        }

        [Fact]
        public async Task GetFilteredAuditLogsAsync_NoFilters_ReturnsAll()
        {
            await using var ctx = await _db.CreateContextAsync();
            ctx.AuditLogs.Add(new AuditLog { AuditLogId = 1, TableName = "Episodes", Action = "Create", UserId = 1, ChangedAt = DateTime.UtcNow });
            ctx.AuditLogs.Add(new AuditLog { AuditLogId = 2, TableName = "Guests", Action = "Update", UserId = 1, ChangedAt = DateTime.UtcNow });
            await ctx.SaveChangesAsync();

            var result = await _sut.GetFilteredAuditLogsAsync(cancellationToken: CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().Contain(i => i.AuditLogId == 1);
            result.Value.Items.Should().Contain(i => i.AuditLogId == 2);
        }

        [Fact]
        public async Task GetFilteredAuditLogsAsync_FilterByTable_ReturnsFiltered()
        {
            await using var ctx = await _db.CreateContextAsync();
            ctx.AuditLogs.Add(new AuditLog { AuditLogId = 10, TableName = "Episodes", Action = "Create", UserId = 1, ChangedAt = DateTime.UtcNow });
            ctx.AuditLogs.Add(new AuditLog { AuditLogId = 11, TableName = "Guests", Action = "Update", UserId = 1, ChangedAt = DateTime.UtcNow });
            await ctx.SaveChangesAsync();

            var result = await _sut.GetFilteredAuditLogsAsync(tableName: "Episodes", cancellationToken: CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().ContainSingle();
            result.Value.Items.Should().Contain(i => i.AuditLogId == 10);
        }

        [Fact]
        public async Task GetFilteredAuditLogsAsync_FilterByUser_ReturnsFiltered()
        {
            await using var ctx = await _db.CreateContextAsync();
            ctx.Users.Add(new ApplicationUser { Id = 99, UserName = "test", FullName = "Test", PasswordHash = "hash", RoleId = 1, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            ctx.AuditLogs.Add(new AuditLog { AuditLogId = 20, TableName = "Episodes", Action = "Create", UserId = 1, ChangedAt = DateTime.UtcNow });
            ctx.AuditLogs.Add(new AuditLog { AuditLogId = 21, TableName = "Episodes", Action = "Create", UserId = 99, ChangedAt = DateTime.UtcNow });
            await ctx.SaveChangesAsync();

            var result = await _sut.GetFilteredAuditLogsAsync(userId: 99, cancellationToken: CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().ContainSingle();
            result.Value.Items[0].AuditLogId.Should().Be(21);
        }

        [Fact]
        public async Task GetFilteredAuditLogsAsync_FilterByDate_ReturnsFiltered()
        {
            await using var ctx = await _db.CreateContextAsync();
            ctx.AuditLogs.Add(new AuditLog { AuditLogId = 30, TableName = "Episodes", Action = "Create", UserId = 1, ChangedAt = new DateTime(2024, 1, 1) });
            ctx.AuditLogs.Add(new AuditLog { AuditLogId = 31, TableName = "Episodes", Action = "Create", UserId = 1, ChangedAt = new DateTime(2024, 6, 15) });
            await ctx.SaveChangesAsync();

            var result = await _sut.GetFilteredAuditLogsAsync(fromDate: new DateTime(2024, 6, 1), toDate: new DateTime(2024, 6, 30), cancellationToken: CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().ContainSingle();
            result.Value.Items[0].AuditLogId.Should().Be(31);
        }

        [Fact]
        public async Task GetFilteredAuditLogsAsync_Pagination_DoesNotThrow()
        {
            await using var ctx = await _db.CreateContextAsync();
            for (int i = 1; i <= 5; i++)
                ctx.AuditLogs.Add(new AuditLog { AuditLogId = 100 + i, TableName = "Episodes", Action = "Create", UserId = 1, ChangedAt = DateTime.UtcNow.AddDays(-i) });
            await ctx.SaveChangesAsync();

            var result = await _sut.GetFilteredAuditLogsAsync(page: 1, pageSize: 2, cancellationToken: CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().NotBeNull();
        }

        [Fact]
        public async Task GetFilteredAuditLogsAsync_LogWithUser_IncludesUsername()
        {
            await using var ctx = await _db.CreateContextAsync();
            ctx.AuditLogs.Add(new AuditLog { AuditLogId = 40, TableName = "Episodes", Action = "Create", UserId = 1, ChangedAt = DateTime.UtcNow });
            await ctx.SaveChangesAsync();

            var result = await _sut.GetFilteredAuditLogsAsync(cancellationToken: CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().Contain(i => i.AuditLogId == 40);
            result.Value.Items.First(i => i.AuditLogId == 40).Username.Should().Be("admin");
        }

        [Fact]
        public async Task GetAuditUsersAsync_ReturnsActiveUsers()
        {
            await using var ctx = await _db.CreateContextAsync();
            ctx.Users.Add(new ApplicationUser { Id = 50, UserName = "active1", FullName = "Active One", PasswordHash = "hash", RoleId = 1, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            ctx.Users.Add(new ApplicationUser { Id = 51, UserName = "inactive1", FullName = "Inactive One", PasswordHash = "hash", RoleId = 1, IsActive = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            await ctx.SaveChangesAsync();

            var result = await _sut.GetAuditUsersAsync(CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Contain(u => u.UserName == "active1");
            result.Value.Should().NotContain(u => u.UserName == "inactive1");
        }
    }
}
