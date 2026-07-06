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
        surface: 'var(--surface)',
        'surface-2': 'var(--surface-2)',
        'surface-3': 'var(--surface-3)',
        border: 'var(--border)',
        ink: { DEFAULT: 'var(--ink)', muted: 'var(--ink-muted)', soft: 'var(--ink-soft)' }
      },
      fontFamily: {
        sans: ['Cairo', 'Tajawal', 'Segoe UI', 'sans-serif']
      },
      borderRadius: {
        xl: '14px'
      }
    }
  },
  plugins: [require('daisyui')],
  daisyui: {
    themes: [
      {
        radiomodern: {
          "primary": "#2563EB",
          "primary-content": "#ffffff",
          "secondary": "#7C3AED",
          "secondary-content": "#ffffff",
          "accent": "#0EA5E9",
          "accent-content": "#ffffff",
          "neutral": "#1E293B",
          "neutral-content": "#F8FAFC",
          "base-100": "#FFFFFF",
          "base-200": "#F8FAFC",
          "base-300": "#F1F5F9",
          "base-content": "#0F172A",
          "info": "#0EA5E9",
          "info-content": "#ffffff",
          "success": "#10B981",
          "success-content": "#ffffff",
          "warning": "#F59E0B",
          "warning-content": "#ffffff",
          "error": "#EF4444",
          "error-content": "#ffffff",
          "--rounded-box": "1rem",
          "--rounded-btn": "0.75rem",
          "--rounded-badge": "1.9rem",
          "--animation-btn": "0.15s",
          "--animation-input": "0.15s",
          "--btn-focus-scale": "0.97",
          "--tab-radius": "0.75rem",
        },
      },
      {
        radiomodernDark: {
          // Surfaces — warm charcoal-gray, not cold black
          "base-100": "#1C1E26",   // page background
          "base-200": "#22252E",   // cards / panels
          "base-300": "#2A2D38",   // hover / elevated
          "base-content": "#E8EAEF", // primary text — warm off-white

          // Primary — soft sky-blue, easy on dark backgrounds
          "primary": "#7EB8FF",
          "primary-content": "#0D1A2E",

          // Secondary — muted lavender
          "secondary": "#A78BFA",
          "secondary-content": "#1A0E33",

          // Accent — calm teal
          "accent": "#38D6EC",
          "accent-content": "#062830",

          // Neutral — matches surface-2 for sidebar/menus
          "neutral": "#22252E",
          "neutral-content": "#9DA4B4",

          // Semantic intent colors — desaturated for dark UI
          "info": "#7EB8FF",
          "info-content": "#0D1A2E",
          "success": "#34D399",
          "success-content": "#022B18",
          "warning": "#FBBF24",
          "warning-content": "#291900",
          "error": "#F87171",
          "error-content": "#2A0808",

          // Shape / animation — unchanged
          "--rounded-box": "1rem",
          "--rounded-btn": "0.75rem",
          "--rounded-badge": "1.9rem",
          "--animation-btn": "0.15s",
          "--animation-input": "0.15s",
          "--btn-focus-scale": "0.97",
          "--tab-radius": "0.75rem",
        },
      },
    ],
    darkTheme: "radiomodernDark",
    themeRoot: ":root",
    logs: false,
  },
};
