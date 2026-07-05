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

// ─── Sidebar submenu toggling ───
function toggleSubmenu(btn) {
    const group = btn.closest('.nav-group');
    if (!group) return;
    group.classList.toggle('open');
    const submenu = group.querySelector('.submenu');
    if (submenu) submenu.classList.toggle('hidden');
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
    if (menu) menu.classList.toggle('hidden');
}

function toggleNotifications(e) {
    e.stopPropagation();
    const dropdown = document.getElementById('notifications-dropdown');
    if (dropdown) dropdown.classList.toggle('hidden');
}

// Close dropdowns on outside click
document.addEventListener('click', function (e) {
    const userMenu = document.getElementById('user-menu');
    const notifDropdown = document.getElementById('notifications-dropdown');
    if (userMenu && !userMenu.contains(e.target) && !e.target.closest('[onclick*="toggleUserMenu"]')) {
        userMenu.classList.add('hidden');
    }
    if (notifDropdown && !notifDropdown.contains(e.target) && !e.target.closest('[onclick*="toggleNotifications"]')) {
        notifDropdown.classList.add('hidden');
    }
});

// ─── Toastr configuration ───
toastr.options = {
    positionClass: 'toast-bottom-left',
    timeOut: 5000,
    extendedTimeOut: 2000,
    closeButton: true,
    progressBar: true,
    rtl: true,
    newestOnTop: false,
    preventDuplicates: false
};

document.addEventListener('DOMContentLoaded', function () {
    console.log('✓ Radio Web MVC initialized');

    // ─── Init Preline ───
    if (window.HSStaticMethods) HSStaticMethods.autoInit();

    // ─── Back-to-top visibility ───
    const backToTop = document.getElementById('back-to-top');
    if (backToTop) {
        window.addEventListener('scroll', function () {
            backToTop.classList.toggle('visible', window.scrollY > 400);
        }, { passive: true });
    }

    // ─── Form loading state ───
    document.addEventListener('submit', function (e) {
        const form = e.target;
        const btn = form.querySelector('[type="submit"]:not(:disabled)');
        if (btn && !form.hasAttribute('data-confirm')) {
            btn.classList.add('btn-loading');
            btn.disabled = true;
        }
    });

    // ─── Global SweetAlert2 Confirmation for Forms ───
    document.addEventListener('submit', function (e) {
        const form = e.target;
        if (form.hasAttribute('data-confirm')) {
            e.preventDefault();
            const message = form.getAttribute('data-confirm');
            Swal.fire({
                title: 'هل أنت متأكد؟',
                text: message,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'نعم، المتابعة',
                cancelButtonText: 'إلغاء',
                customClass: {
                    popup: 'rounded-2xl',
                    confirmButton: 'btn btn-error',
                    cancelButton: 'btn btn-ghost'
                }
            }).then((result) => {
                if (result.isConfirmed) {
                    form.removeAttribute('data-confirm');
                    form.submit();
                }
            });
        }
    });
});
