// ═══════════════════════════════════════════════════════════════════════════
// Sidebar Navigation — Collapse, Submenu, Mobile Drawer
// ═══════════════════════════════════════════════════════════════════════════

(function () {
    'use strict';

    var sidebar = document.getElementById('sidebar');
    if (!sidebar) return;

    var collapseBtn = sidebar.querySelector('.sidebar-toggle');

    /* ─── Desktop Collapse Toggle ─── */

    window.toggleSidebarCollapse = function () {
        var isCollapsed = sidebar.classList.toggle('collapsed');
        sidebar.setAttribute('data-collapsed', isCollapsed ? 'true' : 'false');
        document.body.classList.toggle('sidebar-collapsed', isCollapsed);
        if (collapseBtn) {
            collapseBtn.setAttribute('aria-label', isCollapsed ? 'وسّع القائمة الجانبية' : 'طي القائمة الجانبية');
            collapseBtn.setAttribute('title', isCollapsed ? 'وسّع القائمة' : 'طي القائمة');
        }
        try {
            localStorage.setItem('sidebar-collapsed', isCollapsed ? 'true' : 'false');
        } catch (e) { /* storage unavailable */ }
    };

    /* ─── Restore collapsed state on load ─── */
    try {
        var saved = localStorage.getItem('sidebar-collapsed');
        if (saved === 'true') {
            sidebar.classList.add('collapsed');
            sidebar.setAttribute('data-collapsed', 'true');
            document.body.classList.add('sidebar-collapsed');
            if (collapseBtn) {
                collapseBtn.setAttribute('aria-label', 'وسّع القائمة الجانبية');
                collapseBtn.setAttribute('title', 'وسّع القائمة');
            }
        }
    } catch (e) { /* ignore */ }

    /* ─── Submenu Toggle ─── */

    window.toggleSubmenu = function (btn) {
        var group = btn.closest('.nav-group');
        if (!group) return;

        var isExpanded = group.classList.toggle('expanded');
        btn.setAttribute('aria-expanded', isExpanded ? 'true' : 'false');

        /* In collapsed desktop mode, close other flyouts */
        if (sidebar.classList.contains('collapsed') && window.innerWidth >= 1024) {
            if (isExpanded) {
                sidebar.querySelectorAll('.nav-group.expanded').forEach(function (other) {
                    if (other !== group) {
                        other.classList.remove('expanded');
                        var otherBtn = other.querySelector('.nav-item');
                        if (otherBtn) otherBtn.setAttribute('aria-expanded', 'false');
                    }
                });
            }
        }
    };

    /* ─── Auto-open submenu on page load if a sub-link is active ─── */
    document.addEventListener('DOMContentLoaded', function () {
        sidebar.querySelectorAll('.sub-link.active').forEach(function (link) {
            var group = link.closest('.nav-group');
            if (group) {
                group.classList.add('expanded');
                var btn = group.querySelector('button');
                if (btn) btn.setAttribute('aria-expanded', 'true');
            }
        });
    });

    /* ─── Mobile Drawer ─── */

    var backdrop = document.createElement('div');
    backdrop.className = 'sidebar-backdrop';
    backdrop.setAttribute('aria-hidden', 'true');
    document.body.appendChild(backdrop);

    window.openSidebar = function () {
        sidebar.classList.add('visible');
        backdrop.classList.add('visible');
        document.body.style.overflow = 'hidden';
        /* Focus first focusable element */
        var firstFocusable = sidebar.querySelector('a, button');
        if (firstFocusable) setTimeout(function () { firstFocusable.focus(); }, 100);
    };

    window.closeSidebar = function () {
        sidebar.classList.remove('visible');
        backdrop.classList.remove('visible');
        document.body.style.overflow = '';
    };

    backdrop.addEventListener('click', closeSidebar);

    /* ─── Close mobile sidebar on nav click ─── */
    sidebar.querySelectorAll('a.nav-item').forEach(function (link) {
        link.addEventListener('click', function () {
            if (window.innerWidth < 1024) {
                closeSidebar();
            }
        });
    });

    /* ─── Escape key closes mobile sidebar ─── */
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && sidebar.classList.contains('visible')) {
            closeSidebar();
        }
    });

    /* ─── Handle resize: clean up mobile state ─── */
    window.addEventListener('resize', function () {
        if (window.innerWidth >= 1024 && sidebar.classList.contains('visible')) {
            closeSidebar();
        }
    });

})();
