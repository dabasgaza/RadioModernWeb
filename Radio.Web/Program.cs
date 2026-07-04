// ============================================================
// Program — البرنامج
// ============================================================
// المسؤولية: تعريف البرنامج.
// ============================================================
using DataAccess.Common;
using DataAccess.Data;
using DataAccess.Security;
using DataAccess.Seeding;
using DataAccess.Services;
using DataAccess.Services.Messaging;
using DataAccess.Validation.Validators;
using Domain.Identity;
using Domain.Models;
using FluentValidation;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Radio.Web.HealthChecks;
using Radio.Web.Hubs;
using Radio.Web.Middleware;
using Radio.Web.Security;
using Radio.Web.Services;
using Serilog;
using System.IO.Compression;
using System.Threading.RateLimiting;

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
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredUniqueChars = 4;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
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
    options.EventsType = typeof(CustomCookieAuthenticationEvents); // تحديث الصلاحيات ديناميكياً
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
    var logger = sp.GetRequiredService<ILogger<DbQueryPerformanceInterceptor>>();
    var threshold = config.GetValue<int>("Performance:SlowQueryThresholdMs", 500);
    return new DbQueryPerformanceInterceptor(logger, threshold);
});
builder.Services.AddSingleton<CurrentSessionProvider>();
builder.Services.AddSingleton<AuditInterceptor>();
builder.Services.AddScoped<IMessageService, MvcMessageService>();
builder.Services.AddHostedService<MessageServiceInitializer>();

// --- Application Services ---
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IGuestService, GuestService>();
builder.Services.AddScoped<IGuestQueryService>(sp => sp.GetRequiredService<IGuestService>());
builder.Services.AddScoped<IGuestCommandService>(sp => sp.GetRequiredService<IGuestService>());
builder.Services.AddScoped<ICorrespondentService, CorrespondentService>();
builder.Services.AddScoped<IEpisodeService, EpisodeService>();
builder.Services.AddScoped<IEpisodeQueryService, EpisodeService>();
builder.Services.AddScoped<IEpisodeCommandService, EpisodeService>();
builder.Services.AddScoped<IProgramService, ProgramService>();
builder.Services.AddScoped<IProgramQueryService>(sp => sp.GetRequiredService<IProgramService>());
builder.Services.AddScoped<IProgramCommandService>(sp => sp.GetRequiredService<IProgramService>());
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
builder.Services.AddScoped<IRolePermissionCacheService, RolePermissionCacheService>();
builder.Services.AddScoped<CustomCookieAuthenticationEvents>();
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

// --- Global Exception Handler ---
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// --- Health Checks ---
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: ["db", "critical"]);

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
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    options.Level = CompressionLevel.Fastest);

// --- FluentValidation ---
builder.Services.AddValidatorsFromAssemblyContaining<GuestDtoValidator>();

// --- MVC + Runtime Compilation ---
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "القيمة مطلوبة");
    options.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(_ => "حقل مطلوب");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((x, y) => $"القيمة '{x}' غير صالحة لـ {y}");
})
.AddRazorRuntimeCompilation()
.AddSessionStateTempDataProvider();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

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

    // Migration-first approach: MigrateAsync creates/updates schema, DbSeeder seeds data.
    await using var devMigrateCtx = await devCtxFactory.CreateDbContextAsync();
    await devMigrateCtx.Database.MigrateAsync();
    await DbSeeder.SeedAsync(devCtxFactory, adminPassword);
}

app.UseMiddleware<LogContextMiddleware>();
app.UseSerilogRequestLogging();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseSession();

app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
app.UseExceptionHandler("/Home/Error/500"); // fallback for views (non-API routes)

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

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (ctx, report) =>
    {
        ctx.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            duration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                tags = e.Value.Tags,
                data = e.Value.Data.ToDictionary(d => d.Key, d => d.Value)
            })
        };
        await System.Text.Json.JsonSerializer.SerializeAsync(ctx.Response.Body, result);
    }
});

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


