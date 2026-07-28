(function () {
  var saved, theme;
  try { saved = localStorage.getItem('theme'); } catch (e) {}
  if (saved === 'light' || saved === 'dark' || saved === 'auto') {
    theme = saved;
  } else {
    theme = 'auto';
  }
  var hc = false;
  try { hc = localStorage.getItem('high-contrast') === 'true'; } catch (e) {}
  applyTheme(theme);
  document.documentElement.classList.toggle('high-contrast', hc);

  function applyTheme(t) {
    var isDark;
    if (t === 'auto') {
      isDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    } else {
      isDark = t === 'dark';
    }
    document.documentElement.classList.toggle('dark', isDark);
  }
})();

var RadioTheme = {
  current: function () {
    try { return localStorage.getItem('theme') || 'auto'; } catch (e) { return 'auto'; }
  },
  isDark: function () {
    return document.documentElement.classList.contains('dark');
  },
  isHighContrast: function () {
    return document.documentElement.classList.contains('high-contrast');
  },
  apply: function (theme) {
    var isDark;
    if (theme === 'auto') {
      isDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    } else {
      isDark = theme === 'dark';
    }
    document.documentElement.classList.toggle('dark', isDark);
    try { localStorage.setItem('theme', theme); } catch (e) {}
    document.dispatchEvent(new CustomEvent('radio:theme-change', { detail: { theme: theme } }));
    updateThemeToggleButton();
  },
  cycle: function () {
    var current = this.current();
    var next = current === 'auto' ? 'light' : current === 'light' ? 'dark' : 'auto';
    this.apply(next);
  },
  toggleHighContrast: function () {
    var hc = !this.isHighContrast();
    document.documentElement.classList.toggle('high-contrast', hc);
    try { localStorage.setItem('high-contrast', hc); } catch (e) {}
    document.dispatchEvent(new CustomEvent('radio:contrast-change', { detail: { highContrast: hc } }));
  }
};

function toggleTheme() { RadioTheme.cycle(); }
function toggleHighContrast() { RadioTheme.toggleHighContrast(); }

function updateThemeToggleButton() {
  var btn = document.getElementById('theme-toggle-btn');
  if (!btn) return;
  var icon = btn.querySelector('.material-symbols-rounded');
  if (!icon) return;
  var current = RadioTheme.current();
  if (current === 'auto') {
    icon.textContent = 'brightness_auto';
  } else if (current === 'dark') {
    icon.textContent = 'light_mode';
  } else {
    icon.textContent = 'dark_mode';
  }
  btn.setAttribute('title', current === 'auto' ? 'الوضع التلقائي' : current === 'dark' ? 'الوضع الداكن — اضغط للتبديل' : 'الوضع الفاتح — اضغط للتبديل');
}

document.addEventListener('DOMContentLoaded', function () {
  updateThemeToggleButton();
  window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', function (e) {
    try { var s = localStorage.getItem('theme'); } catch (err) {}
    if (!s || s === 'auto') {
      document.documentElement.classList.toggle('dark', e.matches);
      updateThemeToggleButton();
    }
  });
});

window.RadioTheme = RadioTheme;
