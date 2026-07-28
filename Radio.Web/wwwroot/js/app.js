(function () {
    'use strict';

    window.RadioWeb = {
        copyToClipboard: async function (text) {
            try { await navigator.clipboard.writeText(text); return true; }
            catch { return false; }
        },
        scrollToTop: function () { window.scrollTo({ top: 0, behavior: 'smooth' }); },
        downloadFile: function (url, filename) {
            const a = document.createElement('a');
            a.href = url; a.download = filename;
            document.body.appendChild(a); a.click(); document.body.removeChild(a);
        }
    };

    /* ─── NProgress ─── */
    if (typeof NProgress !== 'undefined') {
        NProgress.configure({ showSpinner: false, minimum: 0.15, speed: 300 });
        let nprogressTimeout;
        document.addEventListener('submit', function (e) {
            const form = e.target;
            if (!form.hasAttribute('data-ignore-loading') && !form.querySelector('[type="submit"]:disabled')) {
                clearTimeout(nprogressTimeout);
                nprogressTimeout = setTimeout(function () { NProgress.start(); }, 150);
            }
        });
        window.addEventListener('beforeunload', function () {
            clearTimeout(nprogressTimeout);
            NProgress.start();
        });
        window.addEventListener('pageshow', function (e) {
            if (e.persisted) { NProgress.done(); }
        });
    }

    /* ─── Modal Helpers ─── */
    function openModal(id) {
        const m = document.getElementById(id);
        if (m) { m.showModal ? m.showModal() : (m.classList.remove('hidden'), m.classList.add('flex')); }
    }
    function closeModal(id) {
        const m = document.getElementById(id);
        if (m) { m.close ? m.close() : (m.classList.add('hidden'), m.classList.remove('flex')); }
    }
    window.openModal = openModal;
    window.closeModal = closeModal;

    /* ─── Dropdown Helpers ─── */
    window.toggleUserMenu = function (e) {
        e.stopPropagation();
        document.getElementById('user-menu')?.classList.toggle('hidden');
    };
    window.toggleNotifications = function (e) {
        e.stopPropagation();
        document.getElementById('notifications-dropdown')?.classList.toggle('hidden');
    };

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

    /* ─── Close dropdowns on Escape ─── */
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            document.getElementById('user-menu')?.classList.add('hidden');
            document.getElementById('notifications-dropdown')?.classList.add('hidden');
        }
    });

    /* ─── Toastr Configuration ─── */
    if (typeof toastr !== 'undefined') {
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
    }

    /* ─── DOM Ready ─── */
    document.addEventListener('DOMContentLoaded', function () {
        if (window.HSStaticMethods) HSStaticMethods.autoInit();

        const backToTop = document.getElementById('back-to-top');
        if (backToTop) {
            window.addEventListener('scroll', function () {
                backToTop.classList.toggle('visible', window.scrollY > 400);
            }, { passive: true });
        }

        document.addEventListener('submit', function (e) {
            const form = e.target;

            if (form.hasAttribute('data-confirm')) {
                e.preventDefault();
                const message = form.getAttribute('data-confirm');
                if (typeof Swal !== 'undefined') {
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
                return;
            }

            const btn = form.querySelector('[type="submit"]:not(:disabled)');
            if (btn && !form.hasAttribute('data-ignore-loading')) {
                btn.classList.add('btn-loading');
                btn.disabled = true;
            }
        });

        /* ─── Copy code blocks ─── */
        document.querySelectorAll('[data-copy]').forEach(function (el) {
            el.addEventListener('click', function () {
                const text = this.getAttribute('data-copy');
                RadioWeb.copyToClipboard(text).then(function (ok) {
                    if (ok && typeof toastr !== 'undefined') {
                        toastr.success('تم النسخ', '', { timeOut: 1500 });
                    }
                });
            });
        });

        console.log('✓ Radio Web initialized');
    });
})();
