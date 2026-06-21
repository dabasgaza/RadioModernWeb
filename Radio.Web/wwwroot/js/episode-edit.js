// ═══════════════════════════════════════════════════════
// Episodes/Edit — Dynamic Rows, Drag & Drop, Helpers
// ═══════════════════════════════════════════════════════

(function () {
    'use strict';

    const EP = {
        addRow(containerId, templateId) {
            const container = document.getElementById(containerId);
            const tpl = document.getElementById(templateId);
            if (!container || !tpl) return;

            const empty = container.querySelector('.ep-empty');
            if (empty) empty.classList.add('hidden');

            const frag = tpl.content.cloneNode(true);
            const row = frag.firstElementChild;
            row.classList.remove('hidden');

            // Re-index all rows
            const rows = container.querySelectorAll('.ep-row');
            const newIdx = rows.length;

            row.querySelectorAll('input, select, textarea, label').forEach(el => {
                const name = el.getAttribute('name');
                const htmlFor = el.getAttribute('for');
                const id = el.getAttribute('id');
                if (name) el.name = name.replace(/\{i\}/g, newIdx);
                if (id) el.id = id.replace(/\{i\}/g, newIdx);
                if (htmlFor) el.setAttribute('for', htmlFor.replace(/\{i\}/g, newIdx));
            });

            container.appendChild(row);
        },

        removeRow(btn) {
            const row = btn.closest('.ep-row');
            if (!row) return;
            row.classList.add('ep-row-leave');
            const container = row.closest('[id$="-container"]');
            setTimeout(() => {
                row.remove();
                EP.reindex(container);
                if (container && !container.querySelector('.ep-row')) {
                    const empty = container.querySelector('.ep-empty');
                    if (empty) empty.classList.remove('hidden');
                }
            }, 250);
        },

        reindex(container) {
            if (!container) return;
            container.querySelectorAll('.ep-row').forEach((row, idx) => {
                row.querySelectorAll('input, select, textarea, label').forEach(el => {
                    const name = el.getAttribute('name');
                    const htmlFor = el.getAttribute('for');
                    const id = el.getAttribute('id');
                    if (name) el.name = name.replace(/\[\d+\]/, `[${idx}]`);
                    if (id) el.id = el.id.replace(/_\d+_/, `_${idx}_`);
                    if (htmlFor) el.setAttribute('for', htmlFor.replace(/_\d+_/, `_${idx}_`));
                });
            });
        },

        initDrag(containerId) {
            const container = document.getElementById(containerId);
            if (!container) return;

            let dragSrc = null;

            container.addEventListener('dragstart', e => {
                const row = e.target.closest('.ep-row');
                if (!row) return;
                dragSrc = row;
                row.classList.add('ep-row-dragging');
                e.dataTransfer.effectAllowed = 'move';
                e.dataTransfer.setData('text/plain', '');
            });

            container.addEventListener('dragend', () => {
                if (dragSrc) dragSrc.classList.remove('ep-row-dragging');
                dragSrc = null;
                container.querySelectorAll('.ep-row').forEach(r => r.classList.remove('ep-row-over'));
            });

            container.addEventListener('dragover', e => {
                e.preventDefault();
                e.dataTransfer.dropEffect = 'move';
                const row = e.target.closest('.ep-row');
                if (row && row !== dragSrc) {
                    container.querySelectorAll('.ep-row').forEach(r => r.classList.remove('ep-row-over'));
                    row.classList.add('ep-row-over');
                }
            });

            container.addEventListener('dragleave', e => {
                const row = e.target.closest('.ep-row');
                if (row) row.classList.remove('ep-row-over');
            });

            container.addEventListener('drop', e => {
                e.preventDefault();
                const target = e.target.closest('.ep-row');
                if (!target || target === dragSrc || !dragSrc) return;
                if (target.compareDocumentPosition(dragSrc) & Node.DOCUMENT_POSITION_FOLLOWING) {
                    target.parentNode.insertBefore(dragSrc, target.nextSibling);
                } else {
                    target.parentNode.insertBefore(dragSrc, target);
                }
                container.querySelectorAll('.ep-row').forEach(r => r.classList.remove('ep-row-over'));
                EP.reindex(container);
            });
        }
    };

    window.EP = EP;

    document.addEventListener('DOMContentLoaded', () => {
        ['guests', 'correspondents', 'employees'].forEach(id => EP.initDrag(id + '-container'));
    });

})();
