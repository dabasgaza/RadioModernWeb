/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: "class",
  content: [
    './Views/**/*.cshtml',
    './wwwroot/js/**/*.js',
    './node_modules/preline/dist/*.js',
  ],
  theme: {
    extend: {
      colors: {
        /* ── Background & Surface ── */
        bg: 'var(--color-bg)',
        'bg-elevated': 'var(--color-bg-elevated)',
        'bg-hover': 'var(--color-bg-hover)',
        surface: 'var(--color-surface)',
        'surface-hover': 'var(--color-surface-hover)',

        /* ── Popover / Tooltip ── */
        popover: 'var(--color-popover)',
        'popover-foreground': 'var(--color-popover-foreground)',

        /* ── Muted ── */
        muted: 'var(--color-muted)',
        'muted-foreground': 'var(--color-muted-foreground)',

        /* ── Sidebar ── */
        sidebar: 'var(--color-sidebar)',
        'sidebar-foreground': 'var(--color-sidebar-foreground)',
        'sidebar-primary': 'var(--color-sidebar-primary)',
        'sidebar-primary-foreground': 'var(--color-sidebar-primary-foreground)',
        'sidebar-accent': 'var(--color-sidebar-accent)',
        'sidebar-accent-foreground': 'var(--color-sidebar-accent-foreground)',
        'sidebar-border': 'var(--color-sidebar-border)',
        'sidebar-ring': 'var(--color-sidebar-ring)',

        /* ── Input ── */
        input: 'var(--color-input)',

        /* ── Borders ── */
        border: 'var(--color-border)',
        'border-light': 'var(--color-border-light)',

        /* ── Text ── */
        text: 'var(--color-text)',
        'text-secondary': 'var(--color-text-secondary)',
        'text-muted': 'var(--color-text-muted)',
        'text-disabled': 'var(--color-text-disabled)',

        /* ── Primary ── */
        primary: 'var(--color-primary)',
        'primary-hover': 'var(--color-primary-hover)',
        'primary-foreground': 'var(--color-primary-foreground)',
        'primary-subtle': 'var(--color-primary-subtle)',
        'primary-border': 'var(--color-primary-border)',
        'primary-light': 'var(--color-primary-light)',

        /* ── Secondary ── */
        secondary: 'var(--color-secondary)',
        'secondary-hover': 'var(--color-secondary-hover)',
        'secondary-foreground': 'var(--color-secondary-foreground)',
        'secondary-subtle': 'var(--color-secondary-subtle)',

        /* ── Accent (Radio flavor) ── */
        accent: 'var(--color-accent)',
        'accent-hover': 'var(--color-accent-hover)',
        'accent-foreground': 'var(--color-accent-foreground)',
        'accent-subtle': 'var(--color-accent-subtle)',

        /* ── Success ── */
        success: 'var(--color-success)',
        'success-hover': 'var(--color-success-hover)',
        'success-foreground': 'var(--color-success-foreground)',
        'success-subtle': 'var(--color-success-subtle)',

        /* ── Warning ── */
        warning: 'var(--color-warning)',
        'warning-hover': 'var(--color-warning-hover)',
        'warning-foreground': 'var(--color-warning-foreground)',
        'warning-subtle': 'var(--color-warning-subtle)',

        /* ── Danger ── */
        danger: 'var(--color-danger)',
        'danger-hover': 'var(--color-danger-hover)',
        'danger-foreground': 'var(--color-danger-foreground)',
        'danger-subtle': 'var(--color-danger-subtle)',

        /* ── Info ── */
        info: 'var(--color-info)',
        'info-hover': 'var(--color-info-hover)',
        'info-foreground': 'var(--color-info-foreground)',
        'info-subtle': 'var(--color-info-subtle)',

        /* ── Live (On-Air Broadcast) ── */
        live: 'var(--color-live)',
        'live-foreground': 'var(--color-live-foreground)',
        'live-subtle': 'var(--color-live-subtle)',

        /* ── Ring / Focus ── */
        ring: 'var(--color-ring)',
        foreground: 'var(--color-text)',

        /* ── Link ── */
        link: 'var(--color-link)',
        'link-hover': 'var(--color-link-hover)',

        /* ── Overlay / Skeleton ── */
        overlay: 'var(--color-overlay)',
        skeleton: 'var(--color-skeleton)',

        /* ── Chart ── */
        'chart-1': 'var(--color-chart-1)',
        'chart-2': 'var(--color-chart-2)',
        'chart-3': 'var(--color-chart-3)',
        'chart-4': 'var(--color-chart-4)',
        'chart-5': 'var(--color-chart-5)',
        'chart-grid': 'var(--color-chart-grid)',

        /* ── Status Badges ── */
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

        /* ── Backward Compatibility ── */
        'text-variant': 'var(--color-text-variant)',
        'bg-hover-low': 'var(--color-bg-hover-low)',
        'bg-hover-high': 'var(--color-bg-hover-high)',
        'text-2': 'var(--color-text-2)',
        'text-3': 'var(--color-text-3)',
        'text-4': 'var(--color-text-4)',
      },
      fontFamily: {
        sans: ['Cairo', 'Almarai', 'Segoe UI', 'sans-serif'],
      },
      borderRadius: {
        xl: '14px',
        '2xl': '18px',
        '3xl': '24px',
      },
      boxShadow: {
        card: 'var(--shadow-card)',
        'card-hover': 'var(--shadow-card-hover)',
        float: 'var(--shadow-float)',
        topbar: 'var(--shadow-topbar)',
        modal: 'var(--shadow-modal)',
        dropdown: 'var(--shadow-dropdown)',
      },
      keyframes: {
        'eq-bar': { '0%, 100%': { transform: 'scaleY(0.35)' }, '50%': { transform: 'scaleY(1)' } },
        'onair-pulse': { '0%, 100%': { opacity: '1', transform: 'scale(1)' }, '50%': { opacity: '0.45', transform: 'scale(0.82)' } },
        'fade-in': { from: { opacity: '0' }, to: { opacity: '1' } },
        'slide-up': { from: { opacity: '0', transform: 'translateY(12px)' }, to: { opacity: '1', transform: 'translateY(0)' } },
        'scale-in': { from: { opacity: '0', transform: 'scale(0.95)' }, to: { opacity: '1', transform: 'scale(1)' } },
      },
      animation: {
        'eq-bar': 'eq-bar 1s ease-in-out infinite',
        'onair-pulse': 'onair-pulse 1.6s ease-in-out infinite',
        'fade-in': 'fade-in 0.25s ease-out',
        'slide-up': 'slide-up 0.35s ease-out',
        'scale-in': 'scale-in 0.2s ease-out',
      },
    },
  },
  plugins: [],
};
