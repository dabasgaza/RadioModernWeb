(function () {
  var saved, theme;
  try { saved = localStorage.getItem('theme'); } catch (e) {}
  if (saved === 'light' || saved === 'dark') {
    theme = saved;
  } else {
    theme = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }
  applyTheme(theme);
})();

function applyTheme(theme) {
  var html = document.documentElement;
  var isDark = theme === 'dark';
  html.setAttribute('data-theme', isDark ? 'tvprodDark' : 'tvprod');
  html.classList.toggle('dark', isDark);
}

var TvProdTheme = {
  current: function () {
    return document.documentElement.classList.contains('dark') ? 'dark' : 'light';
  },

  apply: function (theme) {
    applyTheme(theme);
    try { localStorage.setItem('theme', theme); } catch (e) {}
    document.dispatchEvent(new CustomEvent('tvprod:theme-change', { detail: { theme: theme } }));
    updateThemeToggleButton();
  },

  toggle: function () {
    this.apply(this.current() === 'dark' ? 'light' : 'dark');
  },

  useSystem: function () {
    try { localStorage.removeItem('theme'); } catch (e) {}
    var prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    var theme = prefersDark ? 'dark' : 'light';
    applyTheme(theme);
    updateThemeToggleButton();
  }
};

function toggleTheme() {
  TvProdTheme.toggle();
}

function updateThemeToggleButton() {
  var btn = document.getElementById('theme-toggle-btn');
  if (!btn) return;
  var isDark = TvProdTheme.current() === 'dark';
  var icon = btn.querySelector('.material-symbols-rounded');
  if (icon) icon.textContent = isDark ? 'light_mode' : 'dark_mode';
}

document.addEventListener('DOMContentLoaded', function () {
  updateThemeToggleButton();
  window
    .matchMedia('(prefers-color-scheme: dark)')
    .addEventListener('change', function (e) {
      var saved = null;
      try { saved = localStorage.getItem('theme'); } catch (err) {}
      if (!saved) {
        applyTheme(e.matches ? 'dark' : 'light');
        updateThemeToggleButton();
      }
    });
});

window.TvProdTheme = TvProdTheme;
