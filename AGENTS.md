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

## Recent Changes (Sprint 7 — Design System Overhaul)

### Theme & Color Architecture
- **CSS Variables as single source of truth** — `:root` + `html.dark` define all brand tokens (`--primary`, `--secondary`, `--accent`), state tokens (`--success`, `--warning`, `--danger`), surface hierarchy (`--surface`, `--surface-2`, `--surface-3`), content tokens (`--ink`, `--ink-muted`, `--ink-soft`), and workflow status tokens (`--status-planned-*`, `--status-cancelled-*`, etc.)
- **`tailwind.config.js`** custom colors now reference `var(--surface)` etc. — no more hex duplication
- **Removed 110+ hardcoded Tailwind colors** from `.cshtml` views — replaced with daisyUI semantic utilities (`bg-primary/10`, `text-success`, `border-danger/20`)
- **Cleaned up `!important`** from 196 to 97 (-99), removed 69 `html.dark .bg-*` override blocks
- **Status badges** now use CSS variables via `--status-*` tokens — dark mode handled natively, no separate overrides

### Workflow Visualization & Accessibility
- **`_StatusBadge.cshtml`** — shared partial with icons for all 4 workflow axes (episode, execution, social, website), `role="status"` + `aria-hidden="true"` on decorative icons
- **`StatusBadgeViewModel`** — type + status + label + icon override support
- **Removed 3 duplicate `GetStatusCssClass` helper functions** from Razor views

### Design System Documentation
- **`DesignController` + `Design/Index.cshtml`** — live showcase page displaying all components: colors, status badges, buttons, form elements, cards, badges

### daisyUI Migration (Completed)
- **Phase 5 — Buttons (100%)**: `btn-primary` → `btn btn-primary` (50+ files), `btn-secondary` → `btn btn-ghost`, `btn-danger` → `btn btn-error`. All buttons across all .cshtml files
- **Phase 6 — Forms (100%)**: `form-input` → `input input-bordered`, `form-label` → `label`, `select select-bordered`, `textarea textarea-bordered`. All inputs across 40+ files
- **Phase 7 — Tables + Toolbars + Modals (Full)**: Episodes/Index as reference — `table-premium` → `overflow-x-auto` + `table table-zebra`, `empty-state` → `card bg-base-100 shadow-md`, `action-dock-btn` → `btn btn-ghost btn-square btn-sm`, modals → `<dialog class="modal">`. Programs/Index — `breadcrumb` → `breadcrumbs`, `filter-premium` → `card` + `join`, `card-premium` → `card bg-base-100 shadow-md`
- **Phase 9 — CSS Cleanup (Complete)**: Removed from `app.css` — `.btn-primary`, `.btn-secondary`, `.btn-danger`, `.btn-success`, `.btn-warning`, `.btn-info`, `btn-loading`, `.form-input`, `.form-label`, `.input-wrapper`/`.input-icon` sub-rules, `.action-dock`, `.action-dock-btn`, `.icon-btn-*`, `.glass-card`, `.shimmer`, `.skeleton*`, `.stage-*`, `.avatar-initials`, `.card-icon-circle`. CSS reduced from ~1665 to ~1260 lines
- **Final**: Build ✅ + Test 311/312 ✅ (same pre-existing failure)

### Files Changed
- `wwwroot/css/app.css` — 1943 → ~1600 lines, clean variable-driven architecture
- `wwwroot/css/theme-dark-overrides.css` — kept as oklch polyfill (44 lines, unchanged)
- `tailwind.config.js` — `theme.extend.colors` now uses `var()`
- `Views/Shared/_StatusBadge.cshtml` — new shared status partial
- `ViewModels/EpisodeViewModels.cs` — added `StatusBadgeViewModel`
- `Views/Design/Index.cshtml` + `Controllers/DesignController.cs` — new design showcase page
- 24 `.cshtml` files updated — all hardcoded color utilities replaced with semantic tokens

### Build & Test
- **Build**: `dotnet build Radio.Web` then `dotnet test`
- **CSS**: Edit `Radio.Web/css/tailwind.css` + `Radio.Web/wwwroot/css/app.css`, rebuild via `npm run build:css`
- **Tests**: 311/312 passing (1 pre-existing failure in `PublishingControllerTests.Index_SearchFilter_ReturnsFiltered` - test data issue)
