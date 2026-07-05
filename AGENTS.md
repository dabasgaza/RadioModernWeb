# 📡 Radio Broadcast Workflow System — Project Context

نظام إدارة سير عمل البث الإذاعي — ASP.NET Core MVC (.NET 10)

## Quick Commands

```bash
dotnet build Radio.Web              # Build web app
dotnet build                        # Build all (incl. tests)
dotnet test                         # Run all tests
dotnet test --filter "Category=Unit" # Filter tests
dotnet run --project Radio.Web      # Run locally (https://localhost:5001)
```

## Architecture

- **Pattern**: ASP.NET Core MVC + Razor Pages hybrid
- **ORM**: EF Core 10 (Code First, 21 tables)
- **Auth**: ASP.NET Core Identity + Claims-based authorization
- **UI**: Tailwind CSS 4 + Tabulator.js + Chart.js + Flatpickr
- **Real-time**: SignalR + Toastr.js notifications
- **CSS**: Consolidated into `app.css` + dark mode via `html.dark` class
- **CDN libs**: Preline UI, Flatpickr under `wwwroot/lib/`

## Key Conventions

- Controllers inject `IPermissionEvaluationService` for runtime permission checks
- `[HasPermission("PermissionName")]` attribute on controller actions (not `[Authorize(Policy = ...)]`)
- `<permission-check permission="...">` tag helper for conditional UI rendering
- `CurrentUserService.ToUserSession()` loads permissions from `IPermissionEvaluationService`, NOT cookie claims
- `CustomCookieAuthenticationEvents` strips all Permission claims from cookie — auth is server-side only
- `RolePermissionCacheService` caches role→permissions; invalidated on role/permission change
- View models use `EpisodeEditViewModel` pattern in `Radio.Web/ViewModels/`
- All services in `DataAccess/Services/`, DTOs in `DataAccess/DTOs/`
- Arabic RTL UI throughout

## Recent Changes (Sprint 6)

- **Permission evaluation service** (`IPermissionEvaluationService`) — runtime checks with user-level override support
- **User Permission Overrides** — grant/revoke individual permissions per user (`Views/Users/Permissions.cshtml`)
- **Dark theme** — warm charcoal palette, toggle via `html.dark` / `localStorage`
- **CSS consolidation** — removed 8 token/sidebar files into single `app.css`
- **HasPermission tag helper** — `<permission-check permission="...">` for conditional rendering
- **Flatpickr** — date/time pickers in Episode Edit, with Arabic locale
- **Responsive sidebar** — drawer on mobile, persistent on desktop
- **Toastr + SignalR** — real-time notification delivery
- **Coverage/Correspondent UI redesign** — card-based layout with status badges
- **Auth fix** — `CurrentUserService.ToUserSession()` now loads permissions from service, not cookie claims
- **Build**: `dotnet build Radio.Web` then `dotnet test`
- **CSS**: Edit `Radio.Web/css/tailwind.css` + `Radio.Web/wwwroot/css/app.css`, rebuild via `npm run build:css`
