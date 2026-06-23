// ═══════════════════════════════════════════════════════════════════════════
// SignalR Notifications
// ═══════════════════════════════════════════════════════════════════════════

document.addEventListener('DOMContentLoaded', function () {
    if (typeof signalR === 'undefined') {
        console.warn('SignalR client not loaded');
        return;
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/notifications')
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .configureLogging(signalR.LogLevel.Information)
        .build();

    connection.on('NotificationReceived', function (notification) {
        // Display via Toastr
        const type = notification.type === 'EpisodeStatusChanged' ? 'info' :
                     notification.type === 'AuditLogCreated' ? 'success' : 'info';

        if (typeof toastr !== 'undefined') {
            toastr[type](notification.message, notification.title, {
                timeOut: 5000,
                extendedTimeOut: 2000
            });
        }

        // Add to notifications dropdown
        addNotificationToDropdown(notification);

        // Update badge
        const badge = document.getElementById('notification-badge');
        const badgePing = document.getElementById('notification-badge-ping');
        if (badge) {
            const count = parseInt(badge.textContent) || 0;
            badge.textContent = count + 1;
            badge.classList.remove('hidden');
            if (badgePing) badgePing.classList.remove('hidden');
        }
    });

    function addNotificationToDropdown(notification) {
        const list = document.getElementById('notifications-list');
        if (!list) return;

        // Remove "no notifications" message if present
        const empty = list.querySelector('.text-center');
        if (empty && empty.textContent.includes('لا توجد')) {
            empty.remove();
        }

        const item = document.createElement('div');
        item.className = 'p-3 border-b border-border hover:bg-surface-3 cursor-pointer';
        item.innerHTML = `
            <div class="font-semibold text-sm text-ink">${notification.title}</div>
            <div class="text-xs text-ink-muted mt-1">${notification.message}</div>
            <div class="text-xs text-ink-soft mt-1">${new Date(notification.timestamp).toLocaleString('ar-SA')}</div>
        `;
        list.prepend(item);
    }

    window.clearNotifications = function () {
        const list = document.getElementById('notifications-list');
        if (list) {
            list.innerHTML = '<div class="p-3 text-sm text-ink-soft text-center">لا توجد إشعارات</div>';
        }
        const badge = document.getElementById('notification-badge');
        const badgePing = document.getElementById('notification-badge-ping');
        if (badge) {
            badge.classList.add('hidden');
            badge.textContent = '0';
        }
        if (badgePing) {
            badgePing.classList.add('hidden');
        }
    };

    // Start the connection
    connection.start()
        .then(() => console.log('✓ SignalR connected'))
        .catch(err => console.error('SignalR connection error:', err));

    // Reconnect handler
    connection.onreconnected(connectionId => {
        console.log('✓ SignalR reconnected:', connectionId);
    });

    connection.onclose(error => {
        console.warn('SignalR disconnected:', error);
    });
});
