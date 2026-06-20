using DataAccess.Common;
using DataAccess.Data;
using DataAccess.Security;
using DataAccess.Seeding;
using DataAccess.Services;
using DataAccess.Services.Messaging;
using Domain.Identity;
using Domain.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System.IO.Compression;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Radio.Web;
using Radio.Web.Hubs;
using Radio.Web.Middleware;
using Radio.Web.Security;
using Radio.Web.Services;
using Serilog;

// ───────────────────────────────────────────────────────────────────────
// Startup
// ───────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

var dpDir = Path.Combine(builder.Environment.ContentRootPath, ".dataprotection-keys"); // ponytail: flat file beats registry in dev; swap to KeyVault/Redis in prod
Directory.CreateDirectory(dpDir);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dpDir));

// --- Core Infrastructure ---
builder.Services.AddSingleton<ConnectionStringProtector>();
builder.Services.AddSingleton<SecureConfigurationProvider>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection is required");

var adminPassword = builder.Configuration["Admin:InitialPassword"]
    ?? SecurePasswordGenerator.Generate();

// DbContext Factory
builder.Services.AddDbContextFactory<BroadcastWorkflowDBContext>(
    (sp, options) =>
    {
        options.UseSqlServer(connectionString, sql =>
        {
            sql.CommandTimeout(30);
            sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });

        var auditInterceptor = sp.GetRequiredService<AuditInterceptor>();
        options.AddInterceptors(auditInterceptor);
    },
    ServiceLifetime.Scoped);

// Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 12;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredUniqueChars = 4;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.SecurityStampValidationInterval = TimeSpan.FromMinutes(30);
})
.AddEntityFrameworkStores<BroadcastWorkflowDBContext>()
.AddUserStore<UserStore<ApplicationUser, ApplicationRole, BroadcastWorkflowDBContext, int>>()
.AddRoleStore<RoleStore<ApplicationRole, BroadcastWorkflowDBContext, int>>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<ApplicationUser>, BCryptPasswordHasher>();
builder.Services.AddScoped<ApplicationUserManager>();
builder.Services.AddScoped<ApplicationRoleManager>();
builder.Services.AddScoped<ApplicationSignInManager>();
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(1);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.IsEssential = true;
});

// --- Authorization ---
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AppPermissions.EpisodeManage, p => p.RequireAssertion(ctx => ctx.User.HasPermission(AppPermissions.EpisodeManage)));
    options.AddPolicy(AppPermissions.EpisodeEdit, p => p.RequireAssertion(ctx => ctx.User.HasPermission(AppPermissions.EpisodeEdit)));
    options.AddPolicy(AppPermissions.EpisodeExecute, p => p.RequireAssertion(ctx => ctx.User.HasPermission(AppPermissions.EpisodeExecute)));
    options.AddPolicy(AppPermissions.EpisodePublish, p => p.RequireAssertion(ctx => ctx.User.HasPermission(AppPermissions.EpisodePublish)));
    options.AddPolicy(AppPermissions.EpisodeDelete, p => p.RequireAssertion(ctx => ctx.User.HasPermission(AppPermissions.EpisodeDelete)));
    options.AddPolicy(AppPermissions.EpisodeRevert, p => p.RequireAssertion(ctx => ctx.User.HasPermission(AppPermissions.EpisodeRevert)));
    options.AddPolicy(AppPermissions.ProgramManage, p => p.RequireAssertion(ctx => ctx.User.HasPermission(AppPermissions.ProgramManage)));
    options.AddPolicy(AppPermissions.GuestManage, p => p.RequireAssertion(ctx => ctx.User.HasPermission(AppPermissions.GuestManage)));
    options.AddPolicy(AppPermissions.StaffManage, p => p.RequireAssertion(ctx => ctx.User.HasPermission(AppPermissions.StaffManage)));
    options.AddPolicy(AppPermissions.CoordinationManage, p => p.RequireAssertion(ctx => ctx.User.HasPermission(AppPermissions.CoordinationManage)));
    options.AddPolicy(AppPermissions.ViewReports, p => p.RequireAssertion(ctx => ctx.User.HasPermission(AppPermissions.ViewReports)));
    options.AddPolicy(AppPermissions.DatabaseManage, p => p.RequireAssertion(ctx => ctx.User.HasPermission(AppPermissions.DatabaseManage)));
    options.AddPolicy(AppPermissions.ViewAuditLogs, p => p.RequireAssertion(ctx => ctx.User.HasPermission(AppPermissions.ViewAuditLogs)));
    options.AddPolicy(AppPermissions.EpisodeWebPublish, p => p.RequireAssertion(ctx => ctx.User.HasPermission(AppPermissions.EpisodeWebPublish)));
    options.AddPolicy(AppPermissions.UserManage, p => p.RequireAssertion(ctx => ctx.User.HasPermission(AppPermissions.UserManage)));
});

// --- Database Interceptors ---
builder.Services.AddSingleton<DbQueryPerformanceInterceptor>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var threshold = config.GetValue<int>("Performance:SlowQueryThresholdMs", 500);
    return new DbQueryPerformanceInterceptor(threshold);
});
builder.Services.AddSingleton<CurrentSessionProvider>();
builder.Services.AddSingleton<AuditInterceptor>();

// Session Capture Middleware
builder.Services.AddScoped<SessionCaptureMiddleware>();
builder.Services.AddScoped<IMessageService, MvcMessageService>();
builder.Services.AddHostedService<MessageServiceInitializer>();

// --- Application Services ---
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IIdentitySynchronizer, IdentitySynchronizer>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGuestService, GuestService>();
builder.Services.AddScoped<ICorrespondentService, CorrespondentService>();
builder.Services.AddScoped<IEpisodeService, EpisodeService>();
builder.Services.AddScoped<IEpisodeQueryService, EpisodeService>();
builder.Services.AddScoped<IEpisodeCommandService, EpisodeService>();
builder.Services.AddScoped<IProgramService, ProgramService>();
builder.Services.AddScoped<IExecutionService, ExecutionService>();
builder.Services.AddScoped<IPublishingService, PublishingService>();
builder.Services.AddScoped<IPublishingQueryService, PublishingService>();
builder.Services.AddScoped<IPublishingCommandService, PublishingService>();
builder.Services.AddScoped<IReportsService, ReportsService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICoverageService, CoverageService>();
builder.Services.AddScoped<ICachedLookupService, CachedLookupService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IPlatformService, PlatformService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IDatabaseManagementService, DatabaseManagementService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<ISystemDiagnosticsService, SystemDiagnosticsService>();
builder.Services.AddHostedService<DatabaseBackupScheduler>();

// --- Localization ---
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new System.Globalization.CultureInfo("ar"),
        new System.Globalization.CultureInfo("ar-SA"),
        new System.Globalization.CultureInfo("en"),
        new System.Globalization.CultureInfo("en-US")
    };

    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("ar");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Insert(0,
        new Microsoft.AspNetCore.Localization.CookieRequestCultureProvider());
});

// --- Caching ---
builder.Services.AddHybridCache();

// --- Application Insights ---
var aiConnStr = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
if (!string.IsNullOrEmpty(aiConnStr))
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = aiConnStr;
    });
}
else
{
    // ponytail: register a no-op TelemetryClient so services don't break without AI
    builder.Services.AddSingleton(_ => new TelemetryClient(new TelemetryConfiguration
    {
        ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://localhost/"
    }));
}

// --- Health Checks ---
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString, name: "sql-server");

// --- Rate Limiting ---
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// --- Response Compression ---
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);

// --- MVC + Runtime Compilation ---
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "القيمة مطلوبة");
    options.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(_ => "حقل مطلوب");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((x, y) => $"القيمة '{x}' غير صالحة لـ {y}");
})
.AddRazorRuntimeCompilation();

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

// SignalR
builder.Services.AddScoped<NotificationService>();
builder.Services.AddSignalR();

var app = builder.Build();

// ───────────────────────────────────────────────────────────────────────
// Middleware Pipeline
// ───────────────────────────────────────────────────────────────────────

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseDeveloperExceptionPage();

    // Auto-seed in development
    using var devScope = app.Services.CreateScope();
    var devServiceProvider = devScope.ServiceProvider;
    var devCtxFactory = devServiceProvider.GetRequiredService<IDbContextFactory<BroadcastWorkflowDBContext>>();
    await DbSeeder.SeedAsync(devCtxFactory);

    // Seed Identity roles first (must precede user role assignment)
    var devUserManager = devServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var devRoleManager = devServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    var devCtx = await devCtxFactory.CreateDbContextAsync();
    await using (devCtx)
    {
        var domainRoles = await devCtx.Roles
            .Where(r => r.IsActive)
            .ToListAsync();
        foreach (var dr in domainRoles)
        {
            if (await devRoleManager.RoleExistsAsync(dr.RoleName))
                continue;
            await devRoleManager.CreateAsync(new ApplicationRole
            {
                Name = dr.RoleName,
                RoleDescription = dr.RoleDescription,
                DomainRoleId = dr.RoleId,
                IsActive = true,
            });
        }

        // Seed Identity users and assign roles
        var domainUsers = await devCtx.Users
            .Where(u => u.IsActive)
            .ToListAsync();
        foreach (var du in domainUsers)
        {
            var existing = await devUserManager.FindByNameAsync(du.Username);
            if (existing != null)
            {
                // ponytail: fix existing Identity user with DomainUserId == 0
                if (existing.DomainUserId == 0)
                {
                    existing.DomainUserId = du.UserId;
                    existing.DomainRoleId = du.RoleId;
                    existing.Email = du.EmailAddress;
                    existing.FullName = du.FullName;
                    existing.DisplayPhoneNumber = du.PhoneNumber;
                    await devUserManager.UpdateAsync(existing);
                    Log.Information("Fixed DomainUserId for {Username} → {Id}", du.Username, du.UserId);
                }
                continue;
            }
            var appUser = new ApplicationUser
            {
                UserName = du.Username,
                Email = du.EmailAddress,
                FullName = du.FullName,
                DisplayPhoneNumber = du.PhoneNumber,
                DomainUserId = du.UserId,
                DomainRoleId = du.RoleId,
                IsActive = true,
                EmailConfirmed = true,
            };
            var result = await devUserManager.CreateAsync(appUser, adminPassword);
            if (!result.Succeeded)
                Log.Warning("Failed to seed Identity user {Username}: {Errors}",
                    du.Username, string.Join(", ", result.Errors.Select(e => e.Description)));
            else
            {
                // ponytail: assign user to their role in Identity so ClaimTypes.Role is populated
                var roleName = await devCtx.Roles
                    .Where(r => r.RoleId == du.RoleId)
                    .Select(r => r.RoleName)
                    .FirstOrDefaultAsync();
                if (roleName != null)
                    await devUserManager.AddToRoleAsync(appUser, roleName);
            }
        }
    }
}

app.UseSerilogRequestLogging();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
app.UseExceptionHandler("/Home/Error/500");

app.UseResponseCompression();
app.UseRateLimiter();

app.UseStaticFiles();

var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(locOptions);

app.UseMiddleware<SessionCaptureMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapHub<NotificationHub>("/hubs/notifications");

app.MapHealthChecks("/health");

// ───────────────────────────────────────────────────────────────────────
// Database Health Check
// ───────────────────────────────────────────────────────────────────────

try
{
    using var healthScope = app.Services.CreateScope();
    var healthCtx = healthScope.ServiceProvider.GetRequiredService<IDbContextFactory<BroadcastWorkflowDBContext>>()
        .CreateDbContext();
    await healthCtx.Database.CanConnectAsync();
    Log.Information("Database connection established successfully.");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Failed to connect to the database. The application cannot start.");
    throw;
}

await app.RunAsync();


