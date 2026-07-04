using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataAccess.Common;
using DataAccess.DTOs;
using DataAccess.Services;
using Domain.Identity;
using Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Radio.Tests.Helpers;
using Radio.Tests.TestData.Builders;
using Radio.Tests.TestData.Fixtures;
using Xunit;

namespace Radio.Tests.Services
{
    public class UserServiceTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _db;
        private readonly IUserService _service;
        private readonly UserSession _admin = UserSessionBuilder.CreateAdmin();

        public UserServiceTests(DatabaseFixture db)
        {
            _db = db;
            
            // تهيئة سياق قاعدة البيانات ومخازن Identity
            var ctx = db.DbContextFactory.CreateDbContext();
            var userStore = new UserStore<ApplicationUser, ApplicationRole, BroadcastWorkflowDBContext, int>(ctx);
            var roleStore = new RoleStore<ApplicationRole, BroadcastWorkflowDBContext, int>(ctx);

            var spCollection = new ServiceCollection();
            spCollection.AddSingleton(db.DbContextFactory);
            var sp = spCollection.BuildServiceProvider();

            var userManager = new UserManager<ApplicationUser>(
                userStore,
                new Microsoft.Extensions.Options.OptionsWrapper<IdentityOptions>(new IdentityOptions()),
                new PasswordHasher<ApplicationUser>(),
                new List<IUserValidator<ApplicationUser>>(),
                new List<IPasswordValidator<ApplicationUser>>(),
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                sp,
                Mock.Of<ILogger<UserManager<ApplicationUser>>>());
            
            var roleManager = new RoleManager<ApplicationRole>(
                roleStore,
                new List<IRoleValidator<ApplicationRole>>(),
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                Mock.Of<ILogger<RoleManager<ApplicationRole>>>());

            var sessionProvider = new CurrentSessionProvider(sp);
            var permissionService = new PermissionService();

            _service = new UserService(
                db.DbContextFactory, 
                sessionProvider, 
                Mock.Of<ILogger<UserService>>(), 
                Mock.Of<IRolePermissionCacheService>(), 
                userManager, 
                roleManager, 
                permissionService);
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsUsers()
        {
            var result = await _service.GetAllUsersAsync(CancellationToken.None);
            result.Should().Contain(u => u.Username == "admin");
        }

        [Fact]
        public async Task CreateUserAsync_Valid_ReturnsSuccess()
        {
            var dto = new UserDto
            {
                Username = "newuser",
                FullName = "New User",
                RoleName = "Admin" // استخدام دور موجود في البذر الأساسي
            };
            var result = await _service.CreateUserAsync(dto, "Password123!", _admin, CancellationToken.None);
            result.ShouldBeSuccess();
        }

        [Fact]
        public async Task CreateUserAsync_DuplicateUsername_ReturnsFail()
        {
            var dto = new UserDto
            {
                Username = "admin",
                FullName = "Admin Copy",
                RoleName = "Admin"
            };
            var result = await _service.CreateUserAsync(dto, "Password123!", _admin, CancellationToken.None);
            result.ShouldBeFailure("موجود");
        }

        [Fact]
        public async Task ToggleUserStatusAsync_DeactivatesUser()
        {
            await using var ctx = await _db.CreateContextAsync();
            ctx.Users.Add(new ApplicationUser
            {
                Id = 100,
                UserName = "target",
                NormalizedUserName = "TARGET",
                FullName = "Target User",
                PasswordHash = "hash",
                Email = "",
                PhoneNumber = "",
                RoleId = 1,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString("D"),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await ctx.SaveChangesAsync();

            var result = await _service.ToggleUserStatusAsync(100, false, _admin, CancellationToken.None);
            result.ShouldBeSuccess();
        }

        [Fact]
        public async Task GetRolesAsync_ReturnsRoles()
        {
            var result = await _service.GetRolesAsync(CancellationToken.None);
            result.Should().NotBeEmpty();
        }

        [Fact]
        public async Task CreateRoleAsync_Valid_ReturnsSuccess()
        {
            var dto = new RoleDto { RoleName = "Editor", RoleDescription = "تحرير المحتوى" };
            var result = await _service.CreateRoleAsync(dto, _admin, CancellationToken.None);
            result.ShouldBeSuccess();
        }

        [Fact]
        public async Task UpdateRoleAsync_Valid_ReturnsSuccess()
        {
            var dto = new RoleDto { RoleId = 1, RoleName = "SuperAdmin", RoleDescription = "مسؤول كامل الصلاحيات" };
            var result = await _service.UpdateRoleAsync(dto, _admin, CancellationToken.None);
            result.ShouldBeSuccess();
        }

        [Fact]
        public async Task GetPermissionsMatrixAsync_ReturnsPermissions()
        {
            var result = await _service.GetPermissionsMatrixAsync(1, CancellationToken.None);
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateRolePermissionsAsync_Valid_ReturnsSuccess()
        {
            var result = await _service.UpdateRolePermissionsAsync(1, new List<int> { 1 }, _admin, CancellationToken.None);
            result.ShouldBeSuccess();
        }

        [Fact]
        public async Task CloneRoleAsync_Valid_ClonesRoleAndPermissions()
        {
            // 1) إنشاء دور مصدر
            await using var ctx = await _db.CreateContextAsync();
            var sourceRole = new ApplicationRole
            {
                Name = "SourceForClone",
                NormalizedName = "SOURCEFORCLONE",
                RoleDescription = "دور مصدر للنسخ",
                IsActive = true
            };
            ctx.Roles.Add(sourceRole);
            await ctx.SaveChangesAsync();

            // 2) استنساخ الدور
            var result = await _service.CloneRoleAsync(
                sourceRole.Id,
                "ClonedRoleResult",
                "الوصف المستنسخ",
                _admin,
                CancellationToken.None);

            // 3) التحقق من النجاح ونقل البيانات
            result.ShouldBeSuccess();
            var newRoleId = result.Value;
            newRoleId.Should().BeGreaterThan(0);

            var clonedRole = await ctx.Roles.FindAsync(newRoleId);
            clonedRole.Should().NotBeNull();
            clonedRole!.Name.Should().Be("ClonedRoleResult");
            clonedRole.RoleDescription.Should().Be("الوصف المستنسخ");
        }
    }
}
