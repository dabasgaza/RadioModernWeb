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
- **UI**: Tailwind CSS 3 + Tabulator.js + Chart.js + Flatpickr
- **Real-time**: SignalR + Toastr.js notifications
- **CSS**: `app.css` (design tokens + components) + `site.css` (Tailwind utilities), dark mode via `html.dark`, high-contrast via `html.high-contrast`
- **Themes**: Light (default), Dark (`html.dark`), High Contrast (`html.high-contrast`)
- **CDN libs**: Preline UI, Flatpickr under `wwwroot/lib/`

## Color System (v2 — Premium Design Tokens)

### Philosophy
Inspired by Linear, shadcn/ui, Vercel, Stripe. Elegant neutral palettes with subtle blue primary and warm amber accent (radio broadcast flavor). No bright saturated colors. Premium SaaS aesthetic.

### Theme Support
| Theme | Class | Description |
|-------|-------|-------------|
| Light | (default) | `:root` tokens |
| Dark | `html.dark` | Dark background, light text |
| High Contrast | `html.high-contrast` | WCAG AAA compliant, max readability |

### Token Categories

#### Semantic Colors (for direct use in views)
| Token | Light | Dark | Purpose |
|-------|-------|------|---------|
| `--color-bg` | `#FAFAF9` | `#0C0A09` | Page background |
| `--color-surface` | `#FFFFFF` | `#1C1917` | Card/panel/component surface |
| `--color-surface-hover` | `#F5F5F4` | `#292524` | Hover state surfaces |
| `--color-border` | `#E7E5E4` | `#292524` | Borders, dividers |
| `--color-text` | `#1C1917` | `#F5F5F4` | Primary text |
| `--color-text-secondary` | `#78716C` | `#A8A29E` | Secondary text, labels |
| `--color-text-muted` | `#A8A29E` | `#78716C` | Muted hints, placeholders |
| `--color-primary` | `#2563EB` | `#60A5FA` | Primary actions, links |
| `--color-primary-foreground` | `#FFFFFF` | `#172554` | Text on primary bg |
| `--color-primary-subtle` | `#EFF6FF` | `rgba(96,165,250,0.1)` | Primary backgrounds |
| `--color-accent` | `#D97706` | `#FBBF24` | Accent (radio feel) |
| `--color-success` | `#059669` | `#34D399` | Success states |
| `--color-danger` | `#DC2626` | `#F87171` | Error/danger states |
| `--color-warning` | `#D97706` | `#FBBF24` | Warning states |
| `--color-info` | `#0284C7` | `#38BDF8` | Info states |
| `--color-ring` | `#3B82F6` | `#60A5FA` | Focus ring |

### Tailwind Semantic Classes
Use these instead of raw Tailwind colors:
- `bg-bg`, `bg-surface`, `bg-surface-hover`, `bg-primary`, `bg-primary-subtle`, `bg-danger`, `bg-success`, `bg-warning`, `bg-info`, `bg-accent`
- `text-text`, `text-text-secondary`, `text-text-muted`, `text-text-disabled`, `text-primary`, `text-primary-foreground`
- `border-border`, `border-border-light`
- `ring-ring`

### Status Badge Tokens
- `--color-status-planned-bg/text`: Info variant
- `--color-status-executed-bg/text`: Success variant
- `--color-status-published-bg/text`: Accent variant
- `--color-status-website-bg/text`: Primary variant
- `--color-status-cancelled-bg/text`: Danger variant
- `--color-status-preproduction-bg/text`: Info variant
- `--color-status-filming-bg/text`: Danger variant
- `--color-status-postproduction-bg/text`: Warning variant
- `--color-status-readytoair-bg/text`: Success variant

### Chart Tokens
- `--chart-1`: Primary blue
- `--chart-2`: Accent amber
- `--chart-3`: Success green
- `--chart-4`: Danger red
- `--chart-5`: Info sky
- `--chart-grid`: Border color

## Tailwind Config
Colors defined in `tailwind.config.js` as CSS variable references (e.g., `bg: 'var(--color-bg)'`). The full 50-950 primitive palettes are in `app.css` under `--color-gray-*`, `--color-blue-*`, `--color-amber-*`, `--color-green-*`, `--color-red-*`, `--color-sky-*`.

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
- New views must use semantic classes only — no hardcoded `text-blue-500`, `bg-slate-100`, etc.

## UI/UX Workflow

1. Inspect existing implementation before editing
2. Use semantic Tailwind classes (`text-text`, `bg-surface`, `text-primary`, etc.) — never hardcoded colors
3. Test all 3 themes: light, dark (`html.dark`), high-contrast (`html.high-contrast`)
4. Check: visual hierarchy, spacing, responsive behavior, horizontal overflow, contrast, accessibility, hover/focus/active/disabled states
5. Verify with `dotnet build Radio.Web` then refresh browser (Ctrl+F5)

## Useful Partials

- `_PageHeader` — `ViewData["Title"]`, `ViewData["HeaderIcon"]`, `ViewData["HeaderDescription"]`, `ViewBag.EntityName/EntityUrl`, `ViewBag.BackUrl/BackText`, `ViewData["HeaderActions"]`
- `_EmptyState` — uses `ViewBag.EmptyIcon/Title/Desc/ActionUrl/ActionText/ActionIcon/SecondaryUrl/SecondaryText`
- `_SearchBar` — `ViewBag.SearchValue`, `ViewBag.SearchPlaceholder`, `ViewBag.SearchAction`, `ViewBag.SearchHideCard`
- `_KpiCards` — pass `IEnumerable<dynamic>` with `Label, Value, Sub, Icon, Color`
- `_Skeleton` — types: `card`, `table-row`, `line`, `avatar`
- `_StatusBadge` — status text with color
- `_Pagination` — `.pagination-bar`, `.pagination-page`, `.pagination-nav`

## MCP Tools

- **repowise**: Codebase documentation engine — use `get_answer`, `get_context`, `search_codebase` for architecture and symbol lookup
- **context7**: Fetch up-to-date library docs (Tailwind, daisyUI, EF Core, etc.)

## Stats

- Build: 0 errors
- Tests: 311/312 passing (1 pre-existing `PublishingControllerTests.Index_SearchFilter_ReturnsFiltered` — test data issue)
