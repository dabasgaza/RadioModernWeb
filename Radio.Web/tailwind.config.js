/** @type {import('tailwindcss').Config}
 *
 * Radio Broadcast — Design System v4
 * Tailwind config mapped to semantic CSS custom properties.
 * All colors reference tokens defined in wwwroot/css/app.css.
 */
module.exports = {
  darkMode: 'class',
  content: [
    './Views/**/*.cshtml',
    './wwwroot/js/**/*.js',
  ],
  theme: {
    extend: {
      colors: {
        /* ── Surface Hierarchy ── */
        bg: 'var(--color-bg)',
        'bg-elevated': 'var(--color-bg-elevated)',
        'bg-hover': 'var(--color-bg-hover)',
        'bg-active': 'var(--color-bg-active)',
        surface: 'var(--color-surface)',
        'surface-hover': 'var(--color-surface-hover)',
        'surface-elevated': 'var(--color-surface-elevated)',
        popover: 'var(--color-popover)',
        'popover-foreground': 'var(--color-popover-foreground)',
        muted: 'var(--color-muted)',
        'muted-foreground': 'var(--color-muted-foreground)',
        sidebar: 'var(--color-sidebar)',
        'sidebar-foreground': 'var(--color-sidebar-foreground)',
        'sidebar-hover': 'var(--color-sidebar-hover)',
        topbar: 'var(--color-topbar)',

        /* ── Text (5 levels) ── */
        text: 'var(--color-text)',
        'text-secondary': 'var(--color-text-secondary)',
        'text-muted': 'var(--color-text-muted)',
        'text-disabled': 'var(--color-text-disabled)',
        'text-inverse': 'var(--color-text-inverse)',

        /* ── Border (4 levels) ── */
        border: 'var(--color-border)',
        'border-light': 'var(--color-border-light)',
        'border-strong': 'var(--color-border-strong)',
        'border-focus': 'var(--color-border-focus)',

        /* ── Input ── */
        input: 'var(--color-input)',

        /* ── Primary ── */
        primary: {
          DEFAULT: 'var(--color-primary)',
          hover: 'var(--color-primary-hover)',
          active: 'var(--color-primary-active)',
          foreground: 'var(--color-primary-foreground)',
          subtle: 'var(--color-primary-subtle)',
          border: 'var(--color-primary-border)',
          light: 'var(--color-primary-light)',
        },

        /* ── Secondary ── */
        secondary: {
          DEFAULT: 'var(--color-secondary)',
          hover: 'var(--color-secondary-hover)',
          active: 'var(--color-secondary-active)',
          foreground: 'var(--color-secondary-foreground)',
          subtle: 'var(--color-secondary-subtle)',
        },

        /* ── Accent ── */
        accent: {
          DEFAULT: 'var(--color-accent)',
          hover: 'var(--color-accent-hover)',
          active: 'var(--color-accent-active)',
          foreground: 'var(--color-accent-foreground)',
          subtle: 'var(--color-accent-subtle)',
          border: 'var(--color-accent-border)',
        },

        /* ── Success ── */
        success: {
          DEFAULT: 'var(--color-success)',
          hover: 'var(--color-success-hover)',
          active: 'var(--color-success-active)',
          foreground: 'var(--color-success-foreground)',
          subtle: 'var(--color-success-subtle)',
          border: 'var(--color-success-border)',
        },

        /* ── Warning ── */
        warning: {
          DEFAULT: 'var(--color-warning)',
          hover: 'var(--color-warning-hover)',
          active: 'var(--color-warning-active)',
          foreground: 'var(--color-warning-foreground)',
          subtle: 'var(--color-warning-subtle)',
          border: 'var(--color-warning-border)',
        },

        /* ── Danger ── */
        danger: {
          DEFAULT: 'var(--color-danger)',
          hover: 'var(--color-danger-hover)',
          active: 'var(--color-danger-active)',
          foreground: 'var(--color-danger-foreground)',
          subtle: 'var(--color-danger-subtle)',
          border: 'var(--color-danger-border)',
        },

        /* ── Info ── */
        info: {
          DEFAULT: 'var(--color-info)',
          hover: 'var(--color-info-hover)',
          active: 'var(--color-info-active)',
          foreground: 'var(--color-info-foreground)',
          subtle: 'var(--color-info-subtle)',
          border: 'var(--color-info-border)',
        },

        /* ── Live (On-Air) ── */
        live: {
          DEFAULT: 'var(--color-live)',
          foreground: 'var(--color-live-foreground)',
          subtle: 'var(--color-live-subtle)',
          border: 'var(--color-live-border)',
        },

        /* ── Ring / Focus ── */
        ring: 'var(--color-ring)',
        foreground: 'var(--color-text)',

        /* ── Link ── */
        link: 'var(--color-link)',
        'link-hover': 'var(--color-link-hover)',

        /* ── Overlay / Skeleton / Tooltip ── */
        overlay: 'var(--color-overlay)',
        skeleton: 'var(--color-skeleton)',
        tooltip: 'var(--color-tooltip)',
        'tooltip-foreground': 'var(--color-tooltip-foreground)',

        /* ── Charts ── */
        'chart-1': 'var(--color-chart-1)',
        'chart-2': 'var(--color-chart-2)',
        'chart-3': 'var(--color-chart-3)',
        'chart-4': 'var(--color-chart-4)',
        'chart-5': 'var(--color-chart-5)',
        'chart-6': 'var(--color-chart-6)',
        'chart-grid': 'var(--color-chart-grid)',
        'chart-text': 'var(--color-chart-text)',

        /* ── Status Badges (9 states) ── */
        'status-planned-bg': 'var(--color-status-planned-bg)',
        'status-planned-text': 'var(--color-status-planned-text)',
        'status-executed-bg': 'var(--color-status-executed-bg)',
        'status-executed-text': 'var(--color-status-executed-text)',
        'status-published-bg': 'var(--color-status-published-bg)',
        'status-published-text': 'var(--color-status-published-text)',
        'status-website-bg': 'var(--color-status-website-bg)',
        'status-website-text': 'var(--color-status-website-text)',
        'status-cancelled-bg': 'var(--color-status-cancelled-bg)',
        'status-cancelled-text': 'var(--color-status-cancelled-text)',
        'status-preproduction-bg': 'var(--color-status-preproduction-bg)',
        'status-preproduction-text': 'var(--color-status-preproduction-text)',
        'status-filming-bg': 'var(--color-status-filming-bg)',
        'status-filming-text': 'var(--color-status-filming-text)',
        'status-postproduction-bg': 'var(--color-status-postproduction-bg)',
        'status-postproduction-text': 'var(--color-status-postproduction-text)',
        'status-readytoair-bg': 'var(--color-status-readytoair-bg)',
        'status-readytoair-text': 'var(--color-status-readytoair-text)',

        /* ── Backward compatibility aliases ── */
        'text-variant': 'var(--color-text-variant)',
        'bg-hover-low': 'var(--color-bg-hover-low)',
        'bg-hover-high': 'var(--color-bg-hover-high)',
        'text-2': 'var(--color-text-2)',
        'text-3': 'var(--color-text-3)',
        'text-4': 'var(--color-text-4)',
      },

      fontFamily: {
        sans: ['Cairo', 'Almarai', 'Segoe UI', 'system-ui', 'sans-serif'],
        mono: ['SFMono-Regular', 'Menlo', 'Consolas', 'monospace'],
      },

      fontSize: {
        /* Typography scale mapped to design tokens */
        'display': ['var(--text-display-size)', { lineHeight: 'var(--text-display-lh)', letterSpacing: 'var(--text-display-ls)', fontWeight: 'var(--text-display-weight)' }],
        'headline': ['var(--text-headline-size)', { lineHeight: 'var(--text-headline-lh)', letterSpacing: 'var(--text-headline-ls)', fontWeight: 'var(--text-headline-weight)' }],
        'title-lg': ['var(--text-title-lg-size)', { lineHeight: 'var(--text-title-lg-lh)', letterSpacing: 'var(--text-title-lg-ls)', fontWeight: 'var(--text-title-lg-weight)' }],
        'title': ['var(--text-title-size)', { lineHeight: 'var(--text-title-lh)', letterSpacing: 'var(--text-title-ls)', fontWeight: 'var(--text-title-weight)' }],
        'body-lg': ['var(--text-body-lg-size)', { lineHeight: 'var(--text-body-lg-lh)', fontWeight: 'var(--text-body-lg-weight)' }],
        'body': ['var(--text-body-size)', { lineHeight: 'var(--text-body-lh)', fontWeight: 'var(--text-body-weight)' }],
        'label': ['var(--text-label-size)', { lineHeight: 'var(--text-label-lh)', fontWeight: 'var(--text-label-weight)' }],
        'caption': ['var(--text-caption-size)', { lineHeight: 'var(--text-caption-lh)', fontWeight: 'var(--text-caption-weight)' }],
        'micro': ['var(--text-micro-size)', { lineHeight: 'var(--text-micro-lh)', letterSpacing: 'var(--text-micro-ls)', fontWeight: 'var(--text-micro-weight)' }],
      },

      spacing: {
        '0.5': 'var(--space-1)',
        '1.5': 'var(--space-2)',
        '2.5': 'var(--space-3)',
        '3.5': 'var(--space-4)',
      },

      borderRadius: {
        xs: 'var(--radius-xs)',
        sm: 'var(--radius-sm)',
        md: 'var(--radius-md)',
        lg: 'var(--radius-lg)',
        xl: 'var(--radius-xl)',
        '2xl': 'var(--radius-2xl)',
        '3xl': 'var(--radius-3xl)',
        '4xl': 'var(--radius-4xl)',
      },

      boxShadow: {
        'elevation-0': 'var(--elevation-0)',
        'elevation-1': 'var(--elevation-1)',
        'elevation-2': 'var(--elevation-2)',
        'elevation-3': 'var(--elevation-3)',
        'elevation-4': 'var(--elevation-4)',
        'elevation-5': 'var(--elevation-5)',
        card: 'var(--shadow-card)',
        'card-hover': 'var(--shadow-card-hover)',
        float: 'var(--shadow-float)',
        topbar: 'var(--shadow-topbar)',
        drawer: 'var(--shadow-drawer)',
        modal: 'var(--shadow-modal)',
        dropdown: 'var(--shadow-dropdown)',
      },

      transitionTimingFunction: {
        standard: 'var(--ease-standard)',
        emphasized: 'var(--ease-emphasized)',
        exit: 'var(--ease-exit)',
      },

      transitionDuration: {
        instant: 'var(--duration-instant)',
        fast: 'var(--duration-fast)',
        normal: 'var(--duration-normal)',
        slow: 'var(--duration-slow)',
        slower: 'var(--duration-slower)',
      },

      zIndex: {
        dropdown: 'var(--z-dropdown)',
        sticky: 'var(--z-sticky)',
        sidebar: 'var(--z-sidebar)',
        overlay: 'var(--z-overlay)',
        modal: 'var(--z-modal)',
        toast: 'var(--z-toast)',
        tooltip: 'var(--z-tooltip)',
      },

      keyframes: {
        'eq-bar': { '0%, 100%': { transform: 'scaleY(0.35)' }, '50%': { transform: 'scaleY(1)' } },
        'onair-pulse': { '0%, 100%': { opacity: '1', transform: 'scale(1)' }, '50%': { opacity: '0.45', transform: 'scale(0.82)' } },
        'fade-in': { from: { opacity: '0' }, to: { opacity: '1' } },
        'slide-up': { from: { opacity: '0', transform: 'translateY(12px)' }, to: { opacity: '1', transform: 'translateY(0)' } },
        'scale-in': { from: { opacity: '0', transform: 'scale(0.95)' }, to: { opacity: '1', transform: 'scale(1)' } },
        'page-fade': { from: { opacity: '0', transform: 'translateY(6px)' }, to: { opacity: '1', transform: 'translateY(0)' } },
      },

      animation: {
        'eq-bar': 'eq-bar 1s ease-in-out infinite',
        'onair-pulse': 'onair-pulse 1.6s ease-in-out infinite',
        'fade-in': 'fade-in 0.25s ease-out',
        'slide-up': 'slide-up 0.35s ease-out',
        'scale-in': 'scale-in 0.2s ease-out',
        'page-fade': 'page-fade 0.3s ease-out',
      },
    },
  },
  plugins: [],
};
