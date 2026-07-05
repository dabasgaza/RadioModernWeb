# 📡 Radio Broadcast Workflow System — ASP.NET Core MVC

نظام إدارة سير عمل البث الإذاعي — مبني بـ **.NET 10 + ASP.NET Core MVC + Razor Pages + Tailwind CSS + ASP.NET Core Identity**.

---

## 🎯 نظرة عامة

نسخة ويب احترافية من نظام إدارة سير عمل البث الإذاعي. يستخدم نمط **MVC + Razor Pages هجين** للتوافق مع أفضل الممارسات:

- **MVC Controllers**: للأعمال المعقدة (CRUD + State Management)
- **Razor Pages**: للأعمال البسيطة (Login, Logout, Error)
- **Tailwind CSS**: تصميم احترافي حديث
- **Tabulator.js**: جداول تفاعلية
- **Chart.js**: رسوم بيانية
- **Toastr.js + SignalR**: إشعارات لحظية
- **ASP.NET Core Identity + BCrypt**: مصادقة آمنة

### ✨ التحديثات عن النسخة السابقة (Blazor)

| المحور | السابق (Blazor) | الحالي (MVC) |
|---|---|---|
| Pattern | Blazor Server | **ASP.NET Core MVC + Razor Pages** |
| UI | MudBlazor | **Tailwind CSS 4 + Tabulator + Chart.js** |
| Components | .razor | **.cshtml (Razor Views)** |
| State | Server Circuit | **Stateless HTTP + TempData** |
| Real-time | SignalR (Blazor) | **SignalR + Toastr.js** |

---

## 🏗️ البنية المعمارية

```
RadioModernWeb/
├── Domain/                          # EF Core models + Identity (ApplicationUser, ApplicationRole)
│   ├── Identity/                    # ApplicationUser, ApplicationRole (IdentityUser<int>)
│   ├── Models/                      # 21 entity model + BroadcastWorkflowDBContext
│   ├── Migrations/                  # 22 migration (آخرها AddIdentityTables)
│   └── Domain.csproj
│
├── DataAccess/                      # Services + DTOs + Security + Seeding
│   ├── Services/                    # 23 services (Auth, Episode, Program, Guest, ...)
│   ├── DTOs/                        # Data Transfer Objects
│   ├── Security/                    # ConnectionStringProtector (Data Protection)
│   ├── Seeding/                     # DbSeeder
│   └── Common/                      # AppPermissions, Result, UserSession, SecurityHelper
│
├── Radio.Web/                       # 🆕 ASP.NET Core MVC Project
│   ├── Controllers/                 # 14 controllers
│   │   ├── HomeController.cs        # Dashboard + Chart.js
│   │   ├── AccountController.cs     # Login, Logout, Profile
│   │   ├── EpisodesController.cs    # CRUD + Execute + Cancel + Revert
│   │   ├── ProgramsController.cs
│   │   ├── GuestsController.cs
│   │   ├── CorrespondentsController.cs
│   │   ├── CoverageController.cs    # التغطيات الميدانية
│   │   ├── StaffControllers.cs      # Employees + StaffRoles + SocialPlatforms
│   │   ├── AdminControllers.cs      # Users + Roles + Permissions + AuditLogs
│   │   └── SystemControllers.cs     # Publishing + ExecutionLogs + WebsitePublishing + Reports + Database + Diagnostics
│   │
│   ├── Views/                       # Razor Views (70+ views)
│   │   ├── Shared/
│   │   │   ├── _Layout.cshtml       # Main layout (RTL, Tailwind, dark mode)
│   │   │   ├── _Sidebar.cshtml      # Responsive sidebar drawer
│   │   │   ├── _Topbar.cshtml       # Top bar + user menu + notifications
│   │   │   ├── _Toastr.cshtml       # SignalR + Toastr notification partial
│   │   │   ├── _CreateEditHeader.cshtml
│   │   │   ├── _LoginLayout.cshtml
│   │   │   ├── Error.cshtml
│   │   │   └── AccessDenied.cshtml
│   │   ├── Home/Index.cshtml        # Dashboard with Chart.js
│   │   ├── Account/Login.cshtml
│   │   ├── Episodes/{Index, Details, Edit}.cshtml
│   │   ├── Programs/{Index, Edit}.cshtml
│   │   ├── Guests/{Index, Edit}.cshtml
│   │   ├── Correspondents/{Index, Edit}.cshtml
│   │   ├── Coverage/{Index, Edit}.cshtml
│   │   ├── Employees/{Index, Edit}.cshtml
│   │   ├── StaffRoles/{Index, Edit}.cshtml
│   │   ├── SocialPlatforms/{Index, Edit}.cshtml
│   │   ├── Users/{Index, Edit, Details, Permissions}.cshtml
│   │   ├── Roles/{Index, Edit}.cshtml
│   │   ├── Permissions/Index.cshtml
│   │   ├── AuditLogs/Index.cshtml
│   │   ├── Publishing/{Index, Edit}.cshtml
│   │   ├── ExecutionLogs/{Index, Edit}.cshtml
│   │   ├── WebsitePublishing/{Index, Edit}.cshtml
│   │   ├── Reports/{Index, ByDateRange}.cshtml
│   │   ├── Database/Index.cshtml
│   │   └── Diagnostics/Index.cshtml
│   │
│   ├── Security/
│   │   ├── ApplicationUser.cs
│   │   ├── ApplicationRole.cs
│   │   ├── ApplicationUserClaimsPrincipalFactory.cs
│   │   ├── BCryptPasswordHasher.cs
│   │   ├── ClaimsPrincipalExtensions.cs
│   │   ├── CustomCookieAuthenticationEvents.cs
│   │   ├── HasPermissionAttribute.cs   # [HasPermission("...")] action filter
│   │   ├── PermissionCheckTagHelper.cs # <permission-check permission="..."> tag helper
│   │   ├── PermissionRequirement.cs
│   │   └── HttpContextHolder.cs
│   ├── Services/
│   │   ├── CurrentUserService.cs       # ICurrentUserService implementation
│   │   ├── MvcMessageService.cs
│   │   ├── NotificationService.cs
│   │   └── ViewDataService.cs
│   ├── Hubs/                        # NotificationHub (SignalR)
│   ├── ViewModels/                  # EpisodeEditViewModel, etc.
│   ├── wwwroot/
│   │   ├── css/
│   │   │   ├── site.css             # Tailwind output (generated)
│   │   │   ├── app.css              # All custom styles (consolidated)
│   │   │   ├── flatpickr-theme.css  # Flatpickr dark theme
│   │   │   ├── theme-dark-overrides.css
│   │   │   └── login.css
│   │   ├── js/
│   │   │   ├── app.js               # Helper functions + sidebar toggle
│   │   │   ├── notifications.js     # SignalR client + Toastr integration
│   │   │   ├── episode-edit.js      # Guest/correspondent/employee dynamic rows
│   │   │   ├── datepicker-init.js   # Flatpickr initialization
│   │   │   └── theme-toggle.js      # Dark mode toggle logic
│   │   └── lib/
│   │       ├── flatpickr/           # Flatpickr (datepicker)
│   │       └── preline/             # Preline UI components
│   ├── css/tailwind.css             # Tailwind source (input)
│   ├── tailwind.config.js
│   ├── package.json                 # npm deps (tailwind, postcss)
│   ├── Program.cs                   # MVC + Identity + SignalR + Serilog + permission services
│   └── Radio.Web.csproj
│
└── Radio.Tests/                     # Unit tests
```

---

## 📊 الجداول المغطاة (21 جدول)

كل جدول في النظام له Controller + Views كاملة:

| # | الجدول | Controller | Views |
|---|---|---|---|
| 1 | Episodes (الحلقات) | EpisodesController | Index, Details, Edit |
| 2 | Programs (البرامج) | ProgramsController | Index, Edit |
| 3 | Guests (الضيوف) | GuestsController | Index, Edit |
| 4 | Correspondents (المراسلون) | CorrespondentsController | Index, Edit |
| 5 | CorrespondentCoverage (التغطيات) | CoverageController | Index |
| 6 | Employees (الموظفون) | EmployeesController | Index, Edit |
| 7 | StaffRoles (المسميات) | StaffRolesController | Index, Edit |
| 8 | EpisodeGuest (ضيوف الحلقة) | داخل EpisodesController | Edit |
| 9 | EpisodeCorrespondent (مراسلو الحلقة) | داخل EpisodesController | Edit |
| 10 | EpisodeEmployee (طاقم الحلقة) | داخل EpisodesController | Edit |
| 11 | EpisodeStatus | ReportsController | Index |
| 12 | ExecutionLog (سجل التنفيذ) | ExecutionLogsController | Index |
| 13 | SocialMediaPlatform (المنصات) | SocialPlatformsController | Index, Edit |
| 14 | SocialMediaPublishingLog | PublishingController | Index, LogSocial |
| 15 | SocialMediaPublishingLogPlatform | داخل PublishingController | — |
| 16 | WebsitePublishingLog | WebsitePublishingController | Index, Publish |
| 17 | Users (المستخدمون) | UsersController | Index, Edit |
| 18 | Roles (الأدوار) | RolesController | Index, Edit |
| 19 | Permissions (الصلاحيات) | PermissionsController | Index |
| 20 | RolePermissions | PermissionsController | Index |
| 21 | AuditLogs (التدقيق) | AuditLogsController | Index |
| 22 | DatabaseBackupLog | DatabaseController | Index |

---

## 🚀 التشغيل

### المتطلبات
- **.NET 10 SDK**
- **SQL Server**

### الخطوات

```bash
# 1. استعادة الحزم
dotnet restore

# 2. تحديث ConnectionString في Radio.Web/appsettings.json

# 3. تطبيق المهاجرات وزرع البيانات
dotnet ef database update --project Domain --startup-project Radio.Web

# 4. تشغيل التطبيق
dotnet run --project Radio.Web

# 5. افتح المتصفح على:
#    https://localhost:5001
```

### بيانات الدخول الافتراضية
- **المستخدم**: `admin`
- **كلمة المرور**: `Admin@123`

---

## 🔐 المصادقة والصلاحيات

- **ASP.NET Core Identity** مع `ApplicationUser : IdentityUser<int>`
- **BCryptPasswordHasher** مخصص (متوافق مع الـ hashes الموجودة)
- **[HasPermission("PermissionName")]** attribute على الـ Actions (بدلاً من `[Authorize(Policy = ...)]`)
- **IPermissionEvaluationService** — تقييم لحظي للصلاحيات مع دعم تجاوز الصلاحيات لكل مستخدم
- **User Permission Overrides** — إضافة/إلغاء صلاحيات فردية لمستخدم معين
- **RolePermissionCacheService** — تخزين مؤقت للصلاحيات مع إبطال تلقائي عند التغيير
- **`<permission-check permission="...">`** Tag Helper — عرض مشروط لواجهة المستخدم حسب الصلاحية
- **CustomCookieAuthenticationEvents** يزيل جميع Permission claims من الكوكي — التحقق يتم من الخادوم فقط
- **CurrentUserService.ToUserSession()** تحميل الصلاحيات من `IPermissionEvaluationService` وليس من الكوكي
- **SyncExistingUsersWithIdentityAsync** تزامن تلقائي عند بدء التشغيل

---

## 🎨 التصميم

- **Tailwind CSS 4** (npm build عبر `tailwind.config.js`)
- **Dark Mode** — سمة دافئة بالفحم، تبديل عبر زر في الشريط العلوي، مخزّن في `localStorage`
- **CSS موحَّد** — ملف `app.css` واحد بعد دمج 8 ملفات tokens/sidebar سابقة
- **Flatpickr** — منتقي التاريخ والوقت مع locale عربي
- **Preline UI** — مكونات واجهة إضافية
- **Material Icons** (Google)
- **Cairo + Tajawal** (خطوط عربية)
- **RTL** كامل
- **Modern Blue Palette** (#2563EB primary)

---

## 🛰️ Real-time

- **SignalR Hub** (`/hubs/notifications`)
- **Toastr.js** للإشعارات الفورية
- **SweetAlert2** للحوارات التأكيدية
- **Notification badge** مع ping animation في Topbar
- **Sidebar** متجاوب — درج على الجوال، ثابت على سطح المكتب

---

## 📊 الرسوم البيانية

- **Chart.js** في Dashboard (دوغنات للحالات + أعمدة للبرامج)
- يمكن إضافة المزيد في صفحة Reports

---

## 🧪 الاختبارات

```bash
dotnet test
dotnet test --filter "Category=Unit"   # وحدات فقط
dotnet test --filter "Category=Integration"  # تكامل فقط
```

## 🎨 بناء CSS

```bash
npm run build:css    # بناء Tailwind
# أو للمتابعة المستمرة:
npx tailwindcss -i ./css/tailwind.css -o ./wwwroot/css/site.css --watch
```

---

## 📝 ملاحظات للمطورين

### إضافة Controller جديد
```bash
# مثال: BooksController
dotnet aspnet-codegenerator controller -name BooksController \
    -m Book -dc BroadcastWorkflowDBContext \
    --relativeFolderPath Controllers --useDefaultLayout
```

### إضافة Migration
```bash
dotnet ef migrations add <Name> --project Domain --startup-project Radio.Web
dotnet ef database update --project Domain --startup-project Radio.Web
```

### تخصيص Tailwind للإنتاج
1. `npm install -D tailwindcss @tailwindcss/forms`
2. `npx tailwindcss init`
3. إنشاء `tailwind.config.js` مع نفس theme في `_Layout.cshtml`
4. `npx tailwindcss -i ./input.css -o ./wwwroot/css/tailwind.css --minify`

---

## 🐞 استكشاف الأخطاء

### خطأ في الاتصال بقاعدة البيانات
1. تأكد من تثبيت SQL Server
2. عدّل `ConnectionStrings:DefaultConnection` في `appsettings.json`
3. أو استخدم متغير البيئة: `setx RADIO_CONNECTION_STRING "..."`

### خطأ: Login فاشل
- راجع logs في `logs/radio-web.log`
- تحقق من أن `SyncExistingUsersWithIdentityAsync` شغّلت بنجاح
- ابحث في logs عن "تمت مزامنة Identity User"

### لمراقبة الـ Logs
- Console: تنشأ تلقائياً
- File: `logs/radio-web.log` (rolling daily)
- Seq: `docker run -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest`
