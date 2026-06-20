using DataAccess.Data;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Radio.Tests.TestData.Fixtures;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly DbContextOptions<BroadcastWorkflowDBContext> _options;

    public IDbContextFactory<BroadcastWorkflowDBContext> DbContextFactory { get; }

    public DatabaseFixture()
    {
        _options = new DbContextOptionsBuilder<BroadcastWorkflowDBContext>()
            .UseInMemoryDatabase($"RadioTestDb_{Guid.NewGuid()}")
            .Options;

        DbContextFactory = new TestDbContextFactory(_options);
    }

    public async Task InitializeAsync()
    {
        await using var context = await DbContextFactory.CreateDbContextAsync();
        await SeedBasicDataAsync(context);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private static async Task SeedBasicDataAsync(BroadcastWorkflowDBContext context)
    {
        if (!await context.EpisodeStatuses.AnyAsync())
        {
            context.EpisodeStatuses.AddRange(
                new EpisodeStatus { StatusId = 0, StatusName = "Planned", DisplayName = "مجدولة", SortOrder = 0 },
                new EpisodeStatus { StatusId = 1, StatusName = "Executed", DisplayName = "منفّذة", SortOrder = 1 },
                new EpisodeStatus { StatusId = 2, StatusName = "Published", DisplayName = "منشورة رقمياً", SortOrder = 2 },
                new EpisodeStatus { StatusId = 3, StatusName = "WebsitePublished", DisplayName = "منشورة على الموقع", SortOrder = 3 },
                new EpisodeStatus { StatusId = 4, StatusName = "Cancelled", DisplayName = "ملغاة", SortOrder = 4 }
            );
        }

        if (!await context.SocialMediaPlatforms.AnyAsync())
        {
            context.SocialMediaPlatforms.AddRange(
                new SocialMediaPlatform { SocialMediaPlatformId = 1, Name = "Facebook", Icon = "facebook", BaseUrl = "https://www.facebook.com/" },
                new SocialMediaPlatform { SocialMediaPlatformId = 2, Name = "Twitter", Icon = "twitter", BaseUrl = "https://x.com/" },
                new SocialMediaPlatform { SocialMediaPlatformId = 3, Name = "Instagram", Icon = "instagram", BaseUrl = "https://www.instagram.com/" }
            );
        }

        if (!await context.Roles.AnyAsync())
        {
            var adminRole = new Role { RoleId = 1, RoleName = "Admin", RoleDescription = "مسؤول النظام", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            context.Roles.Add(adminRole);
            context.Users.Add(new User
            {
                UserId = 1, Username = "admin", FullName = "Admin User", PasswordHash = "hash",
                RoleId = 1, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            });
        }

        if (!await context.StaffRoles.AnyAsync())
        {
            context.StaffRoles.AddRange(
                new StaffRole { StaffRoleId = 1, RoleName = "مذيع", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new StaffRole { StaffRoleId = 2, RoleName = "منتج", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );
        }

        if (!await context.Programs.AnyAsync())
        {
            context.Programs.Add(new Domain.Models.Program
            {
                ProgramId = 1, ProgramName = "برنامج اختبار", ProgramDescription = "وصف", IsActive = true,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            });
        }

        if (!await context.Guests.AnyAsync())
        {
            context.Guests.Add(new Guest
            {
                GuestId = 1, FullName = "ضيف اختبار", PhoneNumber = "555", IsActive = true,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            });
        }

        if (!await context.Correspondents.AnyAsync())
        {
            context.Correspondents.Add(new Correspondent
            {
                CorrespondentId = 1, FullName = "مراسل اختبار", IsActive = true,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            });
        }

        if (!await context.Employees.AnyAsync())
        {
            context.Employees.Add(new Employee
            {
                EmployeeId = 1, FullName = "موظف اختبار", StaffRoleId = 1, Notes = "ملاحظة", IsActive = true,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
    }

    public async Task ResetAsync()
    {
        await using var ctx = await CreateContextAsync();
        await ctx.Database.EnsureDeletedAsync();
        await InitializeAsync();
    }

    public async Task<BroadcastWorkflowDBContext> CreateContextAsync()
    {
        var ctx = await DbContextFactory.CreateDbContextAsync();
        ctx.Database.EnsureCreated();
        return ctx;
    }

    private sealed class TestDbContextFactory(DbContextOptions<BroadcastWorkflowDBContext> options)
        : IDbContextFactory<BroadcastWorkflowDBContext>
    {
        public BroadcastWorkflowDBContext CreateDbContext() => new TestBroadcastWorkflowDbContext(options);
        public async Task<BroadcastWorkflowDBContext> CreateDbContextAsync()
            => await Task.FromResult(new TestBroadcastWorkflowDbContext(options));
    }
}
