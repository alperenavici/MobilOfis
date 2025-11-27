// Notification Management JavaScript

// Initialize on page load
document.addEventListener('DOMContentLoaded', function () {
    startNotificationPolling();
    initializeNotificationActions();
    initializeNotificationFilters();
});

// Notification polling
let notificationPollingInterval = null;
const POLLING_INTERVAL = 30000; // 30 seconds

function startNotificationPolling() {
    // Initial fetch
    fetchUnreadCount();

    // Poll periodically
    notificationPollingInterval = setInterval(fetchUnreadCount, POLLING_INTERVAL);
}

function stopNotificationPolling() {
    if (notificationPollingInterval) {
        clearInterval(notificationPollingInterval);
        notificationPollingInterval = null;
    }
}

async function fetchUnreadCount() {
    try {
        const response = await fetch('/api/Notification/unread-count');
        if (response.ok) {
            const data = await response.json();
            updateNotificationBadge(data.count);
        }
    } catch (error) {
        console.error('Error fetching notification count:', error);
    }
}

function updateNotificationBadge(count) {
    const badge = document.querySelector('.notification-badge');
    const headerBadge = document.querySelector('.header-notification-badge');

    if (badge) {
        if (count > 0) {
            badge.textContent = count > 99 ? '99+' : count;
            badge.style.display = 'inline-block';
        } else {
            badge.style.display = 'none';
        }
    }

    if (headerBadge) {
        if (count > 0) {
            headerBadge.textContent = count > 99 ? '99+' : count;
            headerBadge.style.display = 'inline-block';
        } else {
            headerBadge.style.display = 'none';
        }
    }
}

// Notification actions
function initializeNotificationActions() {
    // Mark as read buttons
    document.addEventListener('click', function (e) {
        if (e.target.closest('.mark-as-read-btn')) {
            const btn = e.target.closest('.mark-as-read-btn');
            const notificationId = btn.dataset.notificationId;
            if (notificationId) {
                markAsRead(notificationId);
            }
        }

        // Mark all as read
        if (e.target.closest('.mark-all-as-read-btn')) {
            markAllAsRead();
        }

        // Notification item click
        if (e.target.closest('.notification-item')) {
            const item = e.target.closest('.notification-item');
            const notificationId = item.dataset.notificationId;
            const isUnread = item.classList.contains('unread');

            if (isUnread && notificationId) {
                markAsRead(notificationId);
            }
        }
    });
}

async function markAsRead(notificationId) {
    try {
        const response = await fetch(`/api/Notification/mark-read/${notificationId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            // Update UI
            const notificationItem = document.querySelector(`[data-notification-id="${notificationId}"]`);
            if (notificationItem) {
                notificationItem.classList.remove('unread');
                const badge = notificationItem.querySelector('.unread-badge');
                if (badge) badge.remove();
            }

            // Update count
            fetchUnreadCount();
        }
    } catch (error) {
        console.error('Error marking notification as read:', error);
    }
}

async function markAllAsRead() {
    try {
        const response = await fetch('/api/Notification/mark-all-read', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            showToast('Tüm bildirimler okundu olarak işaretlendi', 'success');

            // Update UI
            document.querySelectorAll('.notification-item.unread').forEach(item => {
                item.classList.remove('unread');
                const badge = item.querySelector('.unread-badge');
                if (badge) badge.remove();
            });

            // Update count
            updateNotificationBadge(0);
        } else {
            showToast('İşlem başarısız oldu', 'error');
        }
    } catch (error) {
        console.error('Error marking all as read:', error);
        showToast('Bir hata oluştu', 'error');
    }
}

// Notification filters
function initializeNotificationFilters() {
    const filterForm = document.getElementById('notificationFilterForm');

    if (filterForm) {
        filterForm.addEventListener('submit', function (e) {
            e.preventDefault();
            applyNotificationFilters();
        });
    }

    // Quick filters
    const quickFilters = document.querySelectorAll('[data-notification-filter]');
    quickFilters.forEach(btn => {
        btn.addEventListener('click', function () {
            const filter = this.dataset.notificationFilter;
            applyQuickFilter(filter);
        });
    });
}

function applyNotificationFilters() {
    const status = document.getElementById('filterStatus')?.value;
    const type = document.getElementById('filterType')?.value;
    const startDate = document.getElementById('filterStartDate')?.value;
    const endDate = document.getElementById('filterEndDate')?.value;

    const params = new URLSearchParams();
    if (status) params.append('status', status);
    if (type) params.append('type', type);
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);

    window.location.href = `/Notification/Index?${params.toString()}`;
}

function applyQuickFilter(filter) {
    const params = new URLSearchParams();

    if (filter === 'unread') {
        params.append('status', 'unread');
    } else if (filter === 'read') {
        params.append('status', 'read');
    } else if (filter === 'today') {
        const today = new Date().toISOString().split('T')[0];
        params.append('startDate', today);
        params.append('endDate', today);
    }

    window.location.href = `/Notification/Index?${params.toString()}`;
}

function clearNotificationFilters() {
    window.location.href = '/Notification/Index';
}

// Notification dropdown (header)
function initializeNotificationDropdown() {
    const dropdown = document.getElementById('notificationDropdown');

    if (dropdown) {
        dropdown.addEventListener('show.bs.dropdown', async function () {
            await loadRecentNotifications();
        });
    }
}

async function loadRecentNotifications() {
    const container = document.getElementById('notificationDropdownContent');

    if (!container) return;

    container.innerHTML = '<div class="text-center py-3"><div class="spinner-border spinner-border-sm"></div></div>';

    try {
        const response = await fetch('/api/Notification/recent');
        if (response.ok) {
            const notifications = await response.json();

            if (notifications.length === 0) {
                container.innerHTML = '<div class="text-center py-3 text-muted">Bildirim yok</div>';
            } else {
                container.innerHTML = notifications.map(n => `
                    <div class="notification-dropdown-item ${n.isRead ? '' : 'unread'}" data-notification-id="${n.id}">
                        <div class="notification-icon ${n.type.toLowerCase()}">
                            <i class="bi bi-${getNotificationIcon(n.type)}"></i>
                        </div>
                        <div class="notification-content">
                            <div class="notification-title">${n.title}</div>
                            <div class="notification-message">${n.message}</div>
                            <div class="notification-time">${formatRelativeTime(n.createdAt)}</div>
                        </div>
                    </div>
                `).join('');
            }
        }
    } catch (error) {
        console.error('Error loading notifications:', error);
        container.innerHTML = '<div class="text-center py-3 text-danger">Bildirimler yüklenemedi</div>';
    }
}

function getNotificationIcon(type) {
    const icons = {
        'Info': 'info-circle',
        'Success': 'check-circle',
        'Warning': 'exclamation-triangle',
        'Error': 'x-circle',
        'Leave': 'calendar-check',
        'Event': 'calendar-event',
        'Salary': 'cash',
        'Post': 'heart-fill'
    };
    return icons[type] || 'bell';
}

function formatRelativeTime(dateString) {
    const date = new Date(dateString);
    const now = new Date();
    const diff = Math.floor((now - date) / 1000); // seconds

    if (diff < 60) return 'Az önce';
    if (diff < 3600) return `${Math.floor(diff / 60)} dakika önce`;
    if (diff < 86400) return `${Math.floor(diff / 3600)} saat önce`;
    if (diff < 604800) return `${Math.floor(diff / 86400)} gün önce`;

    return date.toLocaleDateString('tr-TR');
}

// Export functions
window.notificationFunctions = {
    markAsRead,
    markAllAsRead,
    fetchUnreadCount,
    updateNotificationBadge,
    clearNotificationFilters,
    stopNotificationPolling,
    startNotificationPolling
};

// Initialize dropdown
initializeNotificationDropdown();

// Cleanup on page unload
window.addEventListener('beforeunload', function () {
    stopNotificationPolling();
});

