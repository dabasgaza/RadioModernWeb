using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DataAccess.Common;
using DataAccess.Services;
using Domain.Identity;
using Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Radio.Tests.TestData.Fixtures;
using Xunit;

namespace Radio.Tests.Services
{
    public class PermissionEvaluationServiceTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
    {
        private readonly DatabaseFixture _db;
        private readonly IMemoryCache _cache;
        private readonly Mock<IRolePermissionCacheService> _roleCacheMock;
        private readonly PermissionEvaluationService _sut;

        public PermissionEvaluationServiceTests(DatabaseFixture db)
        {
            _db = db;
            _cache = new MemoryCache(new MemoryCacheOptions());
            _roleCacheMock = new Mock<IRolePermissionCacheService>();
            
            _sut = new PermissionEvaluationService(
                _cache,
                db.DbContextFactory,
                _roleCacheMock.Object,
                Mock.Of<ILogger<PermissionEvaluationService>>()
            );
        }

        public async ValueTask InitializeAsync() => await _db.ResetAsync();
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        [Fact]
        public async Task HasPermission_SuperAdmin_ReturnsTrueAlways()
        {
            // Arrange
            await using var ctx = await _db.CreateContextAsync();
            var user = new ApplicationUser { Id = 20, UserName = "super", FullName = "Super Admin", RoleId = 1, IsActive = true };
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new("DomainUserId", "20"),
                new("DomainRoleId", "1"),
                new("SuperAdmin", "True")
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

            // Act
            var hasAccess = _sut.HasPermission(principal, "Any.Permission.Name");

            // Assert
            hasAccess.Should().BeTrue();
        }

        [Fact]
        public async Task HasPermission_RegularRoleWithPermission_ReturnsTrue()
        {
            // Arrange
            await using var ctx = await _db.CreateContextAsync();
            var role = new ApplicationRole { Id = 3, Name = "Producer", RoleDescription = "منتج", IsActive = true };
            ctx.Roles.Add(role);
            
            var user = new ApplicationUser { Id = 30, UserName = "producer_user", FullName = "Producer User", RoleId = 3, IsActive = true };
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();

            _roleCacheMock.Setup(rc => rc.GetPermissionsForRoleAsync(3))
                          .ReturnsAsync(new List<string> { AppPermissions.EpisodesCreate });

            var claims = new List<Claim>
            {
                new("DomainUserId", "30"),
                new("DomainRoleId", "3")
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

            // Act
            var hasAccess = _sut.HasPermission(principal, AppPermissions.EpisodesCreate);

            // Assert
            hasAccess.Should().BeTrue();
        }

        [Fact]
        public async Task HasPermission_UserHasDenyOverride_ReturnsFalse()
        {
            // Arrange
            await using var ctx = await _db.CreateContextAsync();
            var role = new ApplicationRole { Id = 4, Name = "Editor", RoleDescription = "محرر", IsActive = true };
            ctx.Roles.Add(role);
            
            var user = new ApplicationUser { Id = 40, UserName = "editor_user", FullName = "Editor User", RoleId = 4, IsActive = true };
            ctx.Users.Add(user);
            // Add user exception deny
            ctx.UserClaims.Add(new IdentityUserClaim<int> { UserId = 40, ClaimType = "PermissionDeny", ClaimValue = AppPermissions.EpisodesEdit });
            await ctx.SaveChangesAsync();

            _roleCacheMock.Setup(rc => rc.GetPermissionsForRoleAsync(4))
                          .ReturnsAsync(new List<string> { AppPermissions.EpisodesEdit });

            var claims = new List<Claim>
            {
                new("DomainUserId", "40"),
                new("DomainRoleId", "4")
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

            // Act
            var hasAccess = _sut.HasPermission(principal, AppPermissions.EpisodesEdit);

            // Assert
            hasAccess.Should().BeFalse(); // Deny override takes priority!
        }

        [Fact]
        public async Task HasPermission_UserHasGrantOverride_ReturnsTrue()
        {
            // Arrange
            await using var ctx = await _db.CreateContextAsync();
            var role = new ApplicationRole { Id = 5, Name = "Presenter", RoleDescription = "مذيع", IsActive = true };
            ctx.Roles.Add(role);
            
            var user = new ApplicationUser { Id = 50, UserName = "presenter_user", FullName = "Presenter User", RoleId = 5, IsActive = true };
            ctx.Users.Add(user);
            // Add user exception grant
            ctx.UserClaims.Add(new IdentityUserClaim<int> { UserId = 50, ClaimType = "Permission", ClaimValue = AppPermissions.EpisodesCreate });
            await ctx.SaveChangesAsync();

            _roleCacheMock.Setup(rc => rc.GetPermissionsForRoleAsync(5))
                          .ReturnsAsync(new List<string>());

            var claims = new List<Claim>
            {
                new("DomainUserId", "50"),
                new("DomainRoleId", "5")
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

            // Act
            var hasAccess = _sut.HasPermission(principal, AppPermissions.EpisodesCreate);

            // Assert
            hasAccess.Should().BeTrue(); // Grant override allows it!
        }
    }
}
