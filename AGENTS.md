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

## UI/UX Workflow

For every task involving UI, UX, styling, layout, responsive design,
themes, Razor Views, Tailwind CSS, or daisyUI:

1. Inspect the existing implementation before editing.
2. Use the daisyUI MCP tools to verify appropriate components,
   theme tokens, semantic colors, and recommended patterns.
3. Implement the changes using the project's existing design system.
4. Start or connect to the local ASP.NET Core application.
5. Use Playwright MCP to inspect the actual rendered result.
6. Test relevant pages at desktop and mobile viewport sizes.
7. Check:
   - visual hierarchy
   - spacing consistency
   - responsive behavior
   - horizontal overflow
   - contrast
   - accessibility
   - hover, focus, active, and disabled states
8. Fix discovered issues.
9. Re-run Playwright verification after the fixes.

Do not consider a UI task complete until the rendered result has been
verified with Playwright.

## Recent Changes (Sprint 8 — 10 New Features)

### 1. 📊 Report Export (PDF/Excel)
- **`IReportExportService` + `ReportExportService`** (`Radio.Web/Services/`) — generates Excel via ClosedXML and PDF via QuestPDF for all 3 report views (Index, ByDateRange, Cancelled)
- **Export buttons** added to `Views/Reports/Index.cshtml`, `ByDateRange.cshtml`, `Cancelled.cshtml` — Excel (btn-success) + PDF (btn-error) per page
- **Controller actions**: `ExportIndexExcel`, `ExportIndexPdf`, `ExportDateRangeExcel`, `ExportDateRangePdf`, `ExportCancelledExcel`, `ExportCancelledPdf`
- Dependencies added: `ClosedXML` v0.104.2, `QuestPDF` v2024.12.0

### 2. 🖨️ Print CSS + Buttons
- **`@media print`** styles in `app.css` — hides nav/sidebar/footer/buttons, shows all tab panels, prints tables with borders
- **Print buttons** (`onclick="window.print()"` + `btn btn-ghost`) on all 3 report views

### 3. 🔍 Unified Search
- **`SearchController`** (`/Search?q=...`) — searches Episodes (name/program), Programs (name/category), Guests (name/organization) via `LIKE %query%`
- **`ISearchService` + `SearchService`** in `Radio.Web/Services/` — uses `IDbContextFactory` for data access
- **`SearchViewModels.cs`** — `SearchViewModel` with grouped results
- **`Views/Search/Index.cshtml`** — results grouped by entity type with links to details pages
- **Sidebar** — "بحث عام" link added; topbar search form (`/Search`) now works

### 4. 📅 Monthly Calendar
- **`CalendarController`** — `GET /Calendar` (view) + `GET /Calendar/GetEvents?year=&month=` (JSON endpoint)
- **`Views/Calendar/Index.cshtml`** — pure HTML/JS calendar grid (no external lib), month navigation, day click to show episode list in modal
- **Color-coded episodes** by status (blue=planned, green=executed, red=cancelled, etc.)
- **Sidebar** — "التقويم الشهري" link

### 5. 📎 File Upload (Episode Attachments)
- **`EpisodeAttachmentService`** — stores files in `wwwroot/uploads/{episodeId}/`, metadata in `App_Data/attachments/episode-{episodeId}.json`
- **`FileUploadController`** — `POST /Upload/Episode/{id}` (upload), `GET /Upload/Episode/{id}` (list JSON), `POST /Upload/Delete/{id}/{storedName}`
- **Upload UI** in `Views/Episodes/Details.cshtml` sidebar — file input + upload button, dynamic attachment list with download/delete
- **`wwwroot/uploads/`** directory already existing with `.gitkeep`

### 6. 📧 Email Settings
- **`IEmailService` + `EmailService`** — logs emails to `App_Data/emails/` (MailKit-ready stub); sends when SMTP configured in `appsettings.json`
- **`SettingsController`** — `GET /Settings` shows email config status
- **`Views/Settings/Index.cshtml`** — displays SMTP status + json config template
- **Sidebar** — "الإعدادات" link

### 7. 📋 Production Board (Kanban)
- **`ProductionController`** — groups episodes by StatusId into 5 columns: مجدولة, منفّذة, منشورة, ملغاة, على الموقع
- **`Views/Production/Index.cshtml`** — card-based Kanban layout, click card to go to episode details
- **ViewModels**: `ProductionBoardViewModel`, `BoardColumn`, `ProductionCard` in `SystemViewModels.cs`
- **Sidebar** — "لوحة الإنتاج" link

### 8. 📝 Improved Audit Log
- **Detail column** added to `Views/AuditLogs/Index.cshtml` — eye icon button per row
- **Modal dialog** showing OldValues/NewValues as formatted JSON (syntax highlighted in `<pre>`)
- **JavaScript** parses JSON for pretty-printing; handles null/malformed values

### 9. 📱 Mobile Responsive Tweaks
- **`@media (max-width: 640px)`** in `app.css` — smaller fonts, scrollable tables, compact buttons/cards, full-width modals
- **Utility classes**: `.hide-mobile`, `.stat-card-grid`, `.table-wrapper`

### 10. 💾 Quick Backup Button
- **"نسخ احتياطي سريع"** button in `Views/Database/Index.cshtml` header — posts to `/Database/Backup`
- **"النسخ الاحتياطي"** link added to sidebar under "إدارة النظام" dropdown

### Fix: 503 Static Files (RateLimiter)
- `Program.cs`: Moved `app.UseStaticFiles()` before `app.UseRateLimiter()` — all JS/CSS were returning 503 due to middleware ordering

### New files created:
- `Controllers/SearchController.cs`, `Controllers/CalendarController.cs`, `Controllers/FileUploadController.cs`, `Controllers/ProductionController.cs`, `Controllers/SettingsController.cs`
- `Services/IReportExportService.cs`, `Services/ReportExportService.cs`, `Services/ISearchService.cs`, `Services/SearchService.cs`, `Services/IEpisodeAttachmentService.cs`, `Services/EpisodeAttachmentService.cs`, `Services/IEmailService.cs`, `Services/EmailService.cs`
- `ViewModels/SearchViewModels.cs`
- `Views/Search/Index.cshtml`, `Views/Calendar/Index.cshtml`, `Views/Production/Index.cshtml`, `Views/Settings/Index.cshtml`

### Build & Test
- `dotnet build Radio.Web` — 0 errors
- `dotnet test` — 311/312 passing (1 pre-existing `PublishingControllerTests.Index_SearchFilter_ReturnsFiltered`)

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
