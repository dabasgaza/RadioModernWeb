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
        surface: 'rgb(var(--surface-rgb) / <alpha-value>)',
        'surface-2': 'var(--surface-2)',
        'surface-3': 'var(--surface-3)',
        border: 'rgb(var(--border-rgb) / <alpha-value>)',
        ink: { DEFAULT: 'rgb(var(--ink-rgb) / <alpha-value>)', muted: 'var(--ink-muted)', soft: 'var(--ink-soft)' },
        signal: 'rgb(var(--signal-rgb) / <alpha-value>)',
        live: 'rgb(var(--live-rgb) / <alpha-value>)',
        go: 'rgb(var(--go-rgb) / <alpha-value>)',
        warn: 'rgb(var(--warn-rgb) / <alpha-value>)',
        accent: 'rgb(var(--accent-rgb) / <alpha-value>)',
        'brand-success': 'rgb(var(--success-rgb) / <alpha-value>)',
        'brand-info': 'rgb(var(--info-rgb) / <alpha-value>)',
        'brand-warning': 'rgb(var(--warning-rgb) / <alpha-value>)',
        'brand-error': 'rgb(var(--error-rgb) / <alpha-value>)',
        'brand-purple': 'rgb(var(--purple-rgb) / <alpha-value>)',
        'brand-primary': 'rgb(var(--primary-rgb) / <alpha-value>)',
      },
      fontFamily: {
        sans: ['Cairo', 'Tajawal', 'Segoe UI', 'sans-serif']
      },
      borderRadius: {
        xl: '14px',
        '2xl': '18px',
      },
      boxShadow: {
        card: 'var(--shadow-card)',
        'card-hover': 'var(--shadow-card-hover)',
      },
      keyframes: {
        'eq-bar': {
          '0%, 100%': { transform: 'scaleY(0.35)' },
          '50%': { transform: 'scaleY(1)' },
        },
        'onair-pulse': {
          '0%, 100%': { opacity: '1', transform: 'scale(1)' },
          '50%': { opacity: '0.45', transform: 'scale(0.82)' },
        },
      },
      animation: {
        'eq-bar': 'eq-bar 1s ease-in-out infinite',
        'onair-pulse': 'onair-pulse 1.6s ease-in-out infinite',
      },
    }
  },
  plugins: [require('daisyui')],
  daisyui: {
    themes: [
      {
        radiomodern: {
          "primary": "#2F6BFF",
          "primary-content": "#FFFFFF",
          "secondary": "#18B66B",
          "secondary-content": "#FFFFFF",
          "accent": "#06B6D4",
          "accent-content": "#04222B",
          "neutral": "#5A6678",
          "neutral-content": "#FFFFFF",
          "base-100": "#F8F9FA",
          "base-200": "#EEF1F4",
          "base-300": "#E4E7EB",
          "base-content": "#212529",
          "info": "#2F6BFF",
          "info-content": "#FFFFFF",
          "success": "#18B66B",
          "success-content": "#FFFFFF",
          "warning": "#F4A623",
          "warning-content": "#3A2A00",
          "error": "#FF3B5C",
          "error-content": "#FFFFFF",
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
          "base-100": "#161D28",
          "base-200": "#111721",
          "base-300": "#202A38",
          "base-content": "#EAF0F7",

          "primary": "#5B8CFF",
          "primary-content": "#07122E",

          "secondary": "#2FD47E",
          "secondary-content": "#052E1C",

          "accent": "#22D3EE",
          "accent-content": "#04222B",

          "neutral": "#9AA7B8",
          "neutral-content": "#11161F",

          "info": "#5B8CFF",
          "info-content": "#07122E",
          "success": "#2FD47E",
          "success-content": "#052E1C",
          "warning": "#FFC24B",
          "warning-content": "#3A2A00",
          "error": "#FF5C77",
          "error-content": "#2A0008",

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
