(function () {
  var saved, theme;
  try { saved = localStorage.getItem('theme'); } catch (e) {}
  if (saved === 'light' || saved === 'dark') {
    theme = saved;
  } else {
    theme = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }
  var hc = false;
  try { hc = localStorage.getItem('high-contrast') === 'true'; } catch (e) {}
  document.documentElement.classList.toggle('dark', theme === 'dark');
  document.documentElement.classList.toggle('high-contrast', hc);
})();

var RadioTheme = {
  current: function () {
    return document.documentElement.classList.contains('dark') ? 'dark' : 'light';
  },
  isHighContrast: function () {
    return document.documentElement.classList.contains('high-contrast');
  },
  apply: function (theme) {
    document.documentElement.classList.toggle('dark', theme === 'dark');
    try { localStorage.setItem('theme', theme); } catch (e) {}
    document.dispatchEvent(new CustomEvent('radio:theme-change', { detail: { theme: theme } }));
    updateThemeToggleButton();
  },
  toggle: function () {
    this.apply(this.current() === 'dark' ? 'light' : 'dark');
  },
  toggleHighContrast: function () {
    var hc = !this.isHighContrast();
    document.documentElement.classList.toggle('high-contrast', hc);
    try { localStorage.setItem('high-contrast', hc); } catch (e) {}
    document.dispatchEvent(new CustomEvent('radio:contrast-change', { detail: { highContrast: hc } }));
  },
  useSystem: function () {
    try { localStorage.removeItem('theme'); } catch (e) {}
    var prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    document.documentElement.classList.toggle('dark', prefersDark);
    updateThemeToggleButton();
  }
};

function toggleTheme() { RadioTheme.toggle(); }
function toggleHighContrast() { RadioTheme.toggleHighContrast(); }

function updateThemeToggleButton() {
  var btn = document.getElementById('theme-toggle-btn');
  if (!btn) return;
  var icon = btn.querySelector('.material-symbols-rounded');
  if (icon) icon.textContent = RadioTheme.current() === 'dark' ? 'light_mode' : 'dark_mode';
}

document.addEventListener('DOMContentLoaded', function () {
  updateThemeToggleButton();
  window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', function (e) {
    var saved = null;
    try { saved = localStorage.getItem('theme'); } catch (err) {}
    if (!saved) {
      document.documentElement.classList.toggle('dark', e.matches);
      updateThemeToggleButton();
    }
  });
});

window.RadioTheme = RadioTheme;
