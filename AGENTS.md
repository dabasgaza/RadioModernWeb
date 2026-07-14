# 📡 Radio Broadcast Workflow System — Project Context

نظام إدارة سير عمل البث الإذاعي — ASP.NET Core MVC (.NET 10)

## Quick Commands

```bash
dotnet build Radio.Web              # Build web app (rebuilds Tailwind CSS automatically)
dotnet build                        # Build all (incl. tests)
dotnet test                         # Run all tests
dotnet test --filter "Category=Unit"
dotnet run --project Radio.Web      # Run locally (https://localhost:5001)
```

> **Important**: After editing `.cshtml` files, restart the server (`dotnet run`). .NET 10 precompiles Razor views — changes won't reflect until restart + hard refresh (Ctrl+F5).

## Architecture

- **Pattern**: ASP.NET Core MVC + Razor Pages hybrid
- **ORM**: EF Core 10 (Code First, 21 tables)
- **Auth**: ASP.NET Core Identity + Claims-based authorization
- **UI**: Tailwind CSS 3 + daisyUI 4 + Tabulator.js + Chart.js + Flatpickr
- **Real-time**: SignalR + Toastr.js notifications
- **CSS**: `app.css` (design tokens + custom classes) + `site.css` (Tailwind), dark mode via `html.dark`
- **CDN libs**: Preline UI, Flatpickr under `wwwroot/lib/`

## Key Conventions

- Controllers inject `IPermissionEvaluationService` for runtime permission checks
- `[HasPermission("PermissionName")]` attribute on controller actions
- `<permission-check permission="...">` tag helper for conditional UI rendering
- `CurrentUserService.ToUserSession()` loads permissions from `IPermissionEvaluationService`, NOT cookie claims
- `CustomCookieAuthenticationEvents` strips all Permission claims from cookie — auth is server-side only
- `RolePermissionCacheService` caches role→permissions; invalidated on role/permission change
- View models use `EpisodeEditViewModel` pattern in `Radio.Web/ViewModels/`
- All services in `DataAccess/Services/`, DTOs in `DataAccess/DTOs/`
- Arabic RTL UI throughout
- Stat cards in dark theme: decorative `absolute` elements need `z-0` + `pointer-events-none`, content needs `relative z-10` to prevent text overlay

## UI/UX Workflow

1. Inspect existing implementation before editing
2. Use daisyUI semantic tokens (`--primary`, `--success`, etc.) — avoid hardcoded colors
3. Test both light and dark themes (`html.dark`)
4. Check: visual hierarchy, spacing, responsive behavior, horizontal overflow, contrast, accessibility, hover/focus/active/disabled states
5. Verify with `dotnet build Radio.Web` then refresh browser (Ctrl+F5)

## MCP Tools

- **repowise**: Codebase documentation engine — use `get_answer`, `get_context`, `search_codebase` for architecture and symbol lookup
- **context7**: Fetch up-to-date library docs (Tailwind, daisyUI, EF Core, etc.)

## Stats

- Build: 0 errors
- Tests: 311/312 passing (1 pre-existing `PublishingControllerTests.Index_SearchFilter_ReturnsFiltered` — test data issue)
