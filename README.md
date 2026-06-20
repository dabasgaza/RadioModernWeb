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
│   └── Common/                      # AppPermissions, Result, UserSession, ...
│
├── Radio.Web/                       # 🆕 ASP.NET Core MVC Project
│   ├── Controllers/                 # 14 controller
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
│   ├── Views/                       # Razor Views (60+ view)
│   │   ├── Shared/                  # _Layout, _Sidebar, _Topbar, _Toastr, _LoginLayout, Error, AccessDenied
│   │   ├── Home/Index.cshtml        # Dashboard with Chart.js
│   │   ├── Account/Login.cshtml
│   │   ├── Episodes/{Index, Details, Edit}.cshtml
│   │   ├── Programs/{Index, Edit}.cshtml
│   │   ├── Guests/{Index, Edit}.cshtml
│   │   ├── Correspondents/{Index, Edit}.cshtml
│   │   ├── Coverage/Index.cshtml
│   │   ├── Employees/{Index, Edit}.cshtml
│   │   ├── StaffRoles/{Index, Edit}.cshtml
│   │   ├── SocialPlatforms/{Index, Edit}.cshtml
│   │   ├── Users/{Index, Edit}.cshtml
│   │   ├── Roles/{Index, Edit}.cshtml
│   │   ├── Permissions/Index.cshtml
│   │   ├── AuditLogs/Index.cshtml
│   │   ├── Publishing/Index.cshtml
│   │   ├── ExecutionLogs/Index.cshtml
│   │   ├── WebsitePublishing/Index.cshtml
│   │   ├── Reports/Index.cshtml
│   │   ├── Database/Index.cshtml
│   │   └── Diagnostics/Index.cshtml
│   │
│   ├── Security/                    # ApplicationUser, ApplicationRole, BCryptPasswordHasher, ...
│   ├── Services/                    # CurrentUserService, MvcMessageService, NotificationService, ViewDataService
│   ├── Hubs/                        # NotificationHub (SignalR)
│   ├── ViewModels/                  # View-specific DTOs
│   ├── wwwroot/
│   │   ├── css/site.css             # Tailwind theme + custom styles
│   │   ├── css/login.css            # Login page styles
│   │   ├── js/app.js                # Helper functions + sidebar toggle
│   │   └── js/notifications.js      # SignalR client + Toastr integration
│   ├── Program.cs                   # MVC + Identity + SignalR + Serilog setup
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

## 🔐 المصادقة

- **ASP.NET Core Identity** مع `ApplicationUser : IdentityUser<int>`
- **BCryptPasswordHasher** مخصص (متوافق مع الـ hashes الموجودة)
- **Claims-based Authorization** مع Policies لكل صلاحية من `AppPermissions`
- **SyncExistingUsersWithIdentityAsync** تزامن تلقائي عند بدء التشغيل

---

## 🎨 التصميم

- **Tailwind CSS 4** (عبر CDN — للإنتاج: npm build)
- **Material Icons** (Google)
- **Cairo + Tajawal** (خطوط عربية)
- **RTL** كامل
- **Modern Blue Palette** (#2563EB primary)

---

## 🛰️ Real-time

- **SignalR Hub** (`/hubs/notifications`)
- **Toastr.js** للإشعارات الفورية
- **SweetAlert2** للحوارات التأكيدية
- **Notification badge** في Topbar

---

## 📊 الرسوم البيانية

- **Chart.js** في Dashboard (دوغنات للحالات + أعمدة للبرامج)
- يمكن إضافة المزيد في صفحة Reports

---

## 🧪 الاختبارات

```bash
dotnet test
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
