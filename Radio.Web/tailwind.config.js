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
          "primary": "#007BFF",
          "primary-content": "#FFFFFF",
          "secondary": "#28A745",
          "secondary-content": "#FFFFFF",
          "accent": "#17BEBB",
          "accent-content": "#052C2C",
          "neutral": "#35393D",
          "neutral-content": "#F8F9FA",
          "base-100": "#F8F9FA",
          "base-200": "#EEF1F4",
          "base-300": "#E4E7EB",
          "base-content": "#212529",
          "info": "#007BFF",
          "info-content": "#FFFFFF",
          "success": "#28A745",
          "success-content": "#FFFFFF",
          "warning": "#FFC107",
          "warning-content": "#634A00",
          "error": "#DC3545",
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
          "base-100": "#2C3136",
          "base-200": "#212529",
          "base-300": "#343A40",
          "base-content": "#E9EBEC",

          "primary": "#4D9CFF",
          "primary-content": "#002C5C",

          "secondary": "#6FD8A2",
          "secondary-content": "#08230F",

          "accent": "#17BEBB",
          "accent-content": "#052C2C",

          "neutral": "#2C3136",
          "neutral-content": "#ADB5BD",

          "info": "#4D9CFF",
          "info-content": "#002C5C",
          "success": "#28A745",
          "success-content": "#08230F",
          "warning": "#FFC107",
          "warning-content": "#634A00",
          "error": "#DC3545",
          "error-content": "#5E1018",

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
