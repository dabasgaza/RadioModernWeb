// ═══════════════════════════════════════════════════════════════════════════
// Radio Web MVC — Client-side helpers
// ═══════════════════════════════════════════════════════════════════════════

window.RadioWeb = {
    copyToClipboard: async function (text) {
        try { await navigator.clipboard.writeText(text); return true; }
        catch (e) { return false; }
    },

    scrollToTop: function () { window.scrollTo({ top: 0, behavior: 'smooth' }); },

    downloadFile: function (url, filename) {
        const a = document.createElement('a');
        a.href = url; a.download = filename;
        document.body.appendChild(a); a.click(); document.body.removeChild(a);
    },

    printElement: function (elementId) {
        const el = document.getElementById(elementId);
        if (!el) return;
        const w = window.open('', '_blank');
        if (!w) return;
        w.document.write(`
            <html dir="rtl" lang="ar">
            <head><title>طباعة</title>
            <link href="https://fonts.googleapis.com/css2?family=Cairo:wght@300;400;500;600;700&display=swap" rel="stylesheet">
            <style>
                body { font-family: 'Cairo', sans-serif; padding: 20px; }
                table { width: 100%; border-collapse: collapse; }
                th, td { padding: 8px; border: 1px solid #e2e8f0; text-align: right; }
            </style>
            </head>
            <body>${el.innerHTML}</body>
            </html>`);
        w.document.close(); w.focus();
        setTimeout(() => { w.print(); w.close(); }, 500);
    }
};

// ─── Sidebar ───
function openSidebar() {
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('sidebar-overlay');
    if (sidebar) sidebar.classList.add('open');
    if (overlay) overlay.classList.add('open');
    document.body.style.overflow = 'hidden';
}

function closeSidebar() {
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('sidebar-overlay');
    if (sidebar) sidebar.classList.remove('open');
    if (overlay) overlay.classList.remove('open');
    document.body.style.overflow = '';
}

// ─── Sidebar submenu toggling ───
function toggleSubmenu(btn) {
    const group = btn.closest('.nav-group');
    if (!group) return;
    group.classList.toggle('open');
}

// ─── Modal helpers ───
function openModal(id) {
    const m = document.getElementById(id);
    if (m) { m.classList.remove('hidden'); m.classList.add('flex'); }
}

function closeModal(id) {
    const m = document.getElementById(id);
    if (m) { m.classList.add('hidden'); m.classList.remove('flex'); }
}

// ─── User menu & notifications dropdown ───
function toggleUserMenu(e) {
    e.stopPropagation();
    const menu = document.getElementById('user-menu');
    if (menu) menu.classList.toggle('open');
}

function toggleNotifications(e) {
    e.stopPropagation();
    const dropdown = document.getElementById('notifications-dropdown');
    if (dropdown) dropdown.classList.toggle('open');
}

// Close dropdowns on outside click
document.addEventListener('click', function (e) {
    const userMenu = document.getElementById('user-menu');
    const notifDropdown = document.getElementById('notifications-dropdown');
    if (userMenu && !userMenu.contains(e.target) && !e.target.closest('[onclick*="toggleUserMenu"]')) {
        userMenu.classList.remove('open');
    }
    if (notifDropdown && !notifDropdown.contains(e.target) && !e.target.closest('[onclick*="toggleNotifications"]')) {
        notifDropdown.classList.remove('open');
    }
});

document.addEventListener('DOMContentLoaded', function () {
    // Apply toast-premium class
    var container = document.getElementById('toast-container');
    if (container) container.classList.add('toast-premium');
    console.log('✓ Radio Web MVC initialized');
});
