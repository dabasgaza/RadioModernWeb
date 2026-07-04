/**
 * admin-permissions.js — شجرة صلاحيات تفاعلية
 * يعمل مع صفحة Permissions/Index.cshtml
 */

(function () {
    'use strict';

    // ─── الحالة ───
    let filterState = 'all'; // 'all' | 'assigned' | 'unassigned'
    let searchTerm = '';

    // ─── عناصر الصفحة ───
    const permissionItems = document.querySelectorAll('.permission-item');
    const moduleHeaders = document.querySelectorAll('.permission-module-header');
    const selectAllBtns = document.querySelectorAll('[data-action="select-all"]');
    const deselectAllBtns = document.querySelectorAll('[data-action="deselect-all"]');

    // ─── تحديث عدد الصلاحيات الممنوحة لكل وحدة ───
    function updateModuleCounts() {
        document.querySelectorAll('.permission-module').forEach(function (module) {
            const items = module.querySelectorAll('.permission-item');
            const assigned = module.querySelectorAll('.permission-item.assigned').length;
            const total = items.length;
            const countEl = module.querySelector('.module-count');
            if (countEl) {
                countEl.textContent = assigned + ' / ' + total;
            }
        });
    }

    // ─── تطبيق الفلتر والبحث ───
    function applyFilters() {
        permissionItems.forEach(function (item) {
            const isAssigned = item.classList.contains('assigned');
            const permName = item.querySelector('.perm-name')?.textContent?.toLowerCase() || '';
            const permSystem = item.querySelector('.perm-system')?.textContent?.toLowerCase() || '';
            const matchesSearch = !searchTerm || permName.includes(searchTerm) || permSystem.includes(searchTerm);

            let visible = matchesSearch;
            if (visible && filterState === 'assigned') visible = isAssigned;
            if (visible && filterState === 'unassigned') visible = !isAssigned;

            item.style.display = visible ? '' : 'none';
        });

        // إظهار/إخفاء الوحدات الفارغة
        document.querySelectorAll('.permission-module').forEach(function (module) {
            const visibleItems = module.querySelectorAll('.permission-item[style*="display:"]');
            const hasVisible = module.querySelectorAll('.permission-item').length === 0 ||
                Array.from(module.querySelectorAll('.permission-item')).some(function (item) {
                    return item.style.display !== 'none';
                });
            module.style.display = hasVisible ? '' : 'none';
        });
    }

    // ─── تحديد/إلغاء كل صلاحيات الوحدة ───
    function toggleModule(moduleEl, checked) {
        const checkboxes = moduleEl.querySelectorAll('input[type="checkbox"][name="selectedPermissions"]');
        checkboxes.forEach(function (cb) {
            cb.checked = checked;
            const item = cb.closest('.permission-item');
            if (item) {
                item.classList.toggle('assigned', checked);
            }
        });
        updateModuleCounts();
    }

    // ─── تحديد/إلغاء كل الصلاحيات ───
    function toggleAll(checked) {
        document.querySelectorAll('input[type="checkbox"][name="selectedPermissions"]').forEach(function (cb) {
            cb.checked = checked;
            const item = cb.closest('.permission-item');
            if (item) {
                item.classList.toggle('assigned', checked);
            }
        });
        updateModuleCounts();
    }

    // ─── ربط الأحداث ───

    // عناصر الصلاحيات الفردية — <label> يحتوي على checkbox فيفعّل النقر الافتراضي
    // نكتفي بـ change event فقط لمنع النقر المزدوج
    permissionItems.forEach(function (item) {
        const checkbox = item.querySelector('input[type="checkbox"]');
        if (checkbox) {
            checkbox.addEventListener('change', function () {
                item.classList.toggle('assigned', this.checked);
                updateModuleCounts();
            });
        }
    });

    // رؤوس الوحدات — طي/توسيع + تحديد/إلغاء
    moduleHeaders.forEach(function (header) {
        header.addEventListener('click', function (e) {
            // لا نطوي إذا نقرنا على زر
            if (e.target.closest('button')) return;
            const body = this.nextElementSibling;
            if (body) {
                body.style.display = body.style.display === 'none' ? '' : 'none';
                const arrow = this.querySelector('.module-arrow');
                if (arrow) {
                    arrow.textContent = body.style.display === 'none' ? 'expand_more' : 'expand_less';
                }
            }
        });
    });

    // أزرار تحديد/إلغاء الوحدات
    document.querySelectorAll('[data-action="module-select"]').forEach(function (btn) {
        btn.addEventListener('click', function () {
            const moduleEl = this.closest('.permission-module');
            if (moduleEl) toggleModule(moduleEl, true);
        });
    });

    document.querySelectorAll('[data-action="module-deselect"]').forEach(function (btn) {
        btn.addEventListener('click', function () {
            const moduleEl = this.closest('.permission-module');
            if (moduleEl) toggleModule(moduleEl, false);
        });
    });

    // أزرار تحديد/إلغاء الكل
    selectAllBtns.forEach(function (btn) {
        btn.addEventListener('click', function () { toggleAll(true); });
    });

    deselectAllBtns.forEach(function (btn) {
        btn.addEventListener('click', function () { toggleAll(false); });
    });

    // مربع البحث
    const searchInput = document.getElementById('perm-search');
    if (searchInput) {
        searchInput.addEventListener('input', function () {
            searchTerm = this.value.trim().toLowerCase();
            applyFilters();
        });
    }

    // أزرار التصفية
    document.querySelectorAll('[data-filter]').forEach(function (btn) {
        btn.addEventListener('click', function () {
            filterState = this.dataset.filter;
            document.querySelectorAll('[data-filter]').forEach(function (b) {
                b.classList.toggle('active', b.dataset.filter === filterState);
            });
            applyFilters();
        });
    });

    // التهيئة
    updateModuleCounts();
    applyFilters();

})();
