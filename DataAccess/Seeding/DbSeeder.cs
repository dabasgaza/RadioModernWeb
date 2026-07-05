using DataAccess.Common;
using Domain.Identity;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace DataAccess.Seeding
{
    /// <summary>
    /// المصدر الوحيد للبيانات الابتدائية في نظام إدارة بث الراديو.
    /// يدعم بذر جداول Identity (الأدوار، المستخدمين، والصلاحيات كـ Role Claims) بشكل مباشر.
    /// </summary>
    public static class DbSeeder
    {
        public static async Task SeedAsync(IDbContextFactory<BroadcastWorkflowDBContext> dbFactory, string? adminPassword = null)
        {
            await using var context = await dbFactory.CreateDbContextAsync();

            await SeedEpisodeStatusesAsync(context);
            await SeedIdentityRolesAsync(context);
            await SeedIdentityRoleClaimsAsync(context);
            await SeedAdminUserAsync(context, adminPassword);
            await SeedSocialMediaPlatformsAsync(context);
            await SeedStaffRolesAsync(context);
        }

        private static async Task SeedEpisodeStatusesAsync(BroadcastWorkflowDBContext context)
        {
            var existing = await context.EpisodeStatuses.Select(s => s.StatusId).ToListAsync();

            var statuses = new List<EpisodeStatus>
            {
                new() { StatusId = 0, StatusName = "Planned",          DisplayName = "مخطط لها",           SortOrder = 0 },
                new() { StatusId = 1, StatusName = "Executed",         DisplayName = "تم تنفيذها",          SortOrder = 1 },
                new() { StatusId = 2, StatusName = "Published",        DisplayName = "تم نشرها",            SortOrder = 2 },
                new() { StatusId = 3, StatusName = "WebsitePublished", DisplayName = "منشورة على الموقع",   SortOrder = 3 },
                new() { StatusId = 4, StatusName = "Cancelled",        DisplayName = "ملغاة",               SortOrder = 4 },
            };

            var missing = statuses.Where(s => !existing.Contains(s.StatusId)).ToList();
            if (missing.Count > 0)
            {
                context.EpisodeStatuses.AddRange(missing);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedIdentityRolesAsync(BroadcastWorkflowDBContext context)
        {
            var existingRoles = await context.Roles.Select(r => r.Name).ToListAsync();

            var defaultRoles = new List<ApplicationRole>
            {
                new() { Name = "Admin", NormalizedName = "ADMIN", RoleDescription = "مدير النظام كامل الصلاحيات", IsSuperAdmin = true, IsActive = true },
                new() { Name = "Producer", NormalizedName = "PRODUCER", RoleDescription = "منتج برامج ومنسق حلقات", IsSuperAdmin = false, IsActive = true },
                new() { Name = "Executor", NormalizedName = "EXECUTOR", RoleDescription = "منفذ بث — تسجيل التنفيذ فقط", IsSuperAdmin = false, IsActive = true },
                new() { Name = "SocialPublisher", NormalizedName = "SOCIALPUBLISHER", RoleDescription = "ناشر رقمي — نشر على التواصل الاجتماعي", IsSuperAdmin = false, IsActive = true },
                new() { Name = "WebPublisher", NormalizedName = "WEBPUBLISHER", RoleDescription = "ناشر محتوى على الموقع الرسمي", IsSuperAdmin = false, IsActive = true },
                new() { Name = "Reporter", NormalizedName = "REPORTER", RoleDescription = "مراسل ميداني ومعد تقارير", IsSuperAdmin = false, IsActive = true }
            };

            var missingRoles = defaultRoles.Where(r => !existingRoles.Contains(r.Name)).ToList();
            if (missingRoles.Count > 0)
            {
                context.Roles.AddRange(missingRoles);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedIdentityRoleClaimsAsync(BroadcastWorkflowDBContext context)
        {
            var fields = typeof(AppPermissions).GetFields(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.FlattenHierarchy);

            var allPermissions = new List<string>();
            foreach (var field in fields)
            {
                if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
                {
                    allPermissions.Add((string)field.GetValue(null)!);
                }
            }

            var roles = await context.Roles.ToDictionaryAsync(r => r.Name!);

            // جلب الـ claims الحالية لتفادي التكرار
            var existingClaims = await context.RoleClaims
                .Where(c => c.ClaimType == "Permission")
                .Select(c => new { c.RoleId, c.ClaimValue })
                .ToListAsync();

            var existingClaimsSet = existingClaims.Select(c => (c.RoleId, c.ClaimValue)).ToHashSet();
            var claimsToInsert = new List<IdentityRoleClaim<int>>();

            void AssignPermissions(string roleName, IEnumerable<string> perms)
            {
                if (!roles.TryGetValue(roleName, out var role)) return;

                foreach (var perm in perms)
                {
                    if (!existingClaimsSet.Contains((role.Id, perm)))
                    {
                        claimsToInsert.Add(new IdentityRoleClaim<int>
                        {
                            RoleId = role.Id,
                            ClaimType = "Permission",
                            ClaimValue = perm
                        });
                    }
                }
            }

            // 1) Admin: له جميع الصلاحيات
            AssignPermissions("Admin", allPermissions);

            // 2) Producer: صلاحيات الإنتاج والتنسيق + المشاهدة
            var producerPerms = new[]
            {
                AppPermissions.ProgramView, AppPermissions.ProgramManage,
                AppPermissions.EpisodeView, AppPermissions.EpisodeManage,
                AppPermissions.EpisodeExecute, AppPermissions.EpisodePublish,
                AppPermissions.EpisodeEdit, AppPermissions.EpisodeDelete,
                AppPermissions.EpisodeRevert,
                AppPermissions.GuestView, AppPermissions.GuestManage,
                AppPermissions.CoordinationView, AppPermissions.CoordinationManage,
                AppPermissions.StaffView, AppPermissions.StaffManage,
                AppPermissions.ViewReports
            };
            AssignPermissions("Producer", producerPerms);

            // 3) Executor: مشاهدة الحلقات + تنفيذ
            var executorPerms = new[]
            {
                AppPermissions.EpisodeView,
                AppPermissions.EpisodeExecute
            };
            AssignPermissions("Executor", executorPerms);

            // 4) SocialPublisher: مشاهدة الحلقات + نشر رقمي
            var socialPublisherPerms = new[]
            {
                AppPermissions.EpisodeView,
                AppPermissions.EpisodePublish
            };
            AssignPermissions("SocialPublisher", socialPublisherPerms);

            // 5) WebPublisher: مشاهدة الحلقات + نشر الموقع + تقارير
            var webPublisherPerms = new[]
            {
                AppPermissions.EpisodeView,
                AppPermissions.EpisodeWebPublish,
                AppPermissions.ViewReports
            };
            AssignPermissions("WebPublisher", webPublisherPerms);

            // 6) Reporter: تنسيق ميداني + تقارير
            var reporterPerms = new[]
            {
                AppPermissions.CoordinationView, AppPermissions.CoordinationManage,
                AppPermissions.ViewReports
            };
            AssignPermissions("Reporter", reporterPerms);

            if (claimsToInsert.Count > 0)
            {
                context.RoleClaims.AddRange(claimsToInsert);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedAdminUserAsync(BroadcastWorkflowDBContext context, string? adminPassword = null)
        {
            var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword ?? "Admin123!");
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null) return;

            var now = DateTime.UtcNow;
            var existingUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == "admin");

            if (existingUser != null)
            {
                existingUser.PasswordHash = adminPasswordHash;
                existingUser.RoleId = adminRole.Id;
                existingUser.UpdatedAt = now;
                existingUser.SecurityStamp = Guid.NewGuid().ToString("D");
                existingUser.FullName = "مسؤول النظام";
                existingUser.IsActive = true;
                existingUser.EmailConfirmed = true;

                var existingUserRole = await context.UserRoles
                    .FirstOrDefaultAsync(ur => ur.UserId == existingUser.Id);
                if (existingUserRole == null)
                {
                    context.UserRoles.Add(new IdentityUserRole<int>
                    {
                        UserId = existingUser.Id,
                        RoleId = adminRole.Id
                    });
                }

                await context.SaveChangesAsync();
                return;
            }

            var adminUser = new ApplicationUser
            {
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@broadcast.pro",
                NormalizedEmail = "ADMIN@BROADCAST.PRO",
                EmailConfirmed = true,
                FullName = "مسؤول النظام",
                IsActive = true,
                RoleId = adminRole.Id,
                CreatedAt = now,
                UpdatedAt = now,
                SecurityStamp = Guid.NewGuid().ToString("D"),
                PasswordHash = adminPasswordHash
            };

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();

            context.UserRoles.Add(new IdentityUserRole<int>
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id
            });

            await context.SaveChangesAsync();
        }

        private static async Task SeedSocialMediaPlatformsAsync(BroadcastWorkflowDBContext context)
        {
            if (await context.SocialMediaPlatforms.AnyAsync())
                return;

            context.SocialMediaPlatforms.AddRange(
                new SocialMediaPlatform { Name = "Facebook", Icon = "Facebook", BaseUrl = "https://www.facebook.com/" },
                new SocialMediaPlatform { Name = "Twitter", Icon = "Twitter", BaseUrl = "https://x.com/" },
                new SocialMediaPlatform { Name = "TikTok", Icon = "MusicNote", BaseUrl = "https://www.tiktok.com/" },
                new SocialMediaPlatform { Name = "YouTube", Icon = "Youtube", BaseUrl = "https://www.youtube.com/watch?v=" },
                new SocialMediaPlatform { Name = "Instagram", Icon = "Instagram", BaseUrl = "https://www.instagram.com/" });

            await context.SaveChangesAsync();
        }

        private static async Task SeedStaffRolesAsync(BroadcastWorkflowDBContext context)
        {
            if (await context.StaffRoles.AnyAsync())
                return;

            context.StaffRoles.AddRange(
                new StaffRole { RoleName = "مذيع" },
                new StaffRole { RoleName = "منفذ" },
                new StaffRole { RoleName = "مهندس صوت" },
                new StaffRole { RoleName = "مخرج" },
                new StaffRole { RoleName = "مصور" });

            await context.SaveChangesAsync();
        }
    }
}
