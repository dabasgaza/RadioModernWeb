/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Views/**/*.cshtml',
    './wwwroot/js/**/*.js'
  ],
  theme: {
    extend: {
      colors: {
        primary: { DEFAULT: '#2563EB', dark: '#1D4ED8', light: '#3B82F6' },
        secondary: { DEFAULT: '#7C3AED', dark: '#6D28D9' },
        accent: '#0EA5E9',
        success: '#10B981',
        warning: '#F59E0B',
        danger: '#EF4444',
        surface: '#FFFFFF',
        'surface-2': '#F8FAFC',
        'surface-3': '#F1F5F9',
        border: '#E2E8F0',
        ink: { DEFAULT: '#0F172A', muted: '#475569', soft: '#94A3B8' }
      },
      fontFamily: {
        sans: ['Cairo', 'Tajawal', 'Segoe UI', 'sans-serif']
      },
      borderRadius: {
        xl: '14px'
      }
    }
  },
  plugins: [],
}
