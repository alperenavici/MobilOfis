// Dashboard JavaScript

// Chart.js instances
let leaveStatusChart = null;
let departmentChart = null;
let salaryTrendChart = null;

// Initialize dashboard
document.addEventListener('DOMContentLoaded', function () {
    // Initial load from API
    loadDashboardData();

    // Start polling
    startNotificationPolling();
    initializeWidgetRefresh();
});

// Load all dashboard data
async function loadDashboardData() {
    await Promise.all([
        loadStats(),
        loadRecentLeaves(),
        loadUpcomingEvents(),
        loadRecentNotifications()
    ]);

    // Load manager/HR specific data if elements exist
    if (document.getElementById('departmentChart')) {
        loadHRStats();
    }
}

// Fetch and display stats
async function loadStats() {
    try {
        const response = await fetch('/api/Dashboard/stats');
        if (!response.ok) return;

        const data = await response.json();

        updateStatCard('pendingLeavesCount', data.pendingLeaves);
        updateStatCard('approvedLeavesCount', data.approvedLeaves);
        updateStatCard('upcomingEventsCount', data.upcomingEvents);
        updateStatCard('unreadNotificationsCount', data.unreadNotifications);

        // Update Chart
        initializeLeaveStatusChart(data.approvedLeaves, data.pendingLeaves, 0); // Rejected count not in stats yet, assuming 0 or fetch separately

    } catch (error) {
        console.error('Error loading stats:', error);
    }
}

function updateStatCard(elementId, value) {
    const element = document.getElementById(elementId);
    if (element) {
        element.textContent = value;
    }
}

// Fetch and display recent leaves
async function loadRecentLeaves() {
    try {
        const response = await fetch('/api/Dashboard/recent-leaves');
        if (!response.ok) return;

        const leaves = await response.json();
        const container = document.getElementById('recentLeavesTableBody');

        if (container) {
            if (leaves.length === 0) {
                container.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Henüz izin talebiniz bulunmuyor</td></tr>';
                return;
            }

            container.innerHTML = leaves.map(leave => `
                <tr>
                    <td>${leave.leavesTypeDisplay || leave.leavesType}</td>
                    <td>${new Date(leave.startDate).toLocaleDateString('tr-TR')}</td>
                    <td>${new Date(leave.endDate).toLocaleDateString('tr-TR')}</td>
                    <td>${leave.dayCount}</td>
                    <td>
                        <span class="badge bg-${leave.statusBadgeColor || getStatusColor(leave.status)}">
                            ${leave.statusDisplay || leave.status}
                        </span>
                    </td>
                    <td>
                        <a href="/Leave/Detail/${leave.leavesId}" class="btn btn-sm btn-outline-secondary">
                            Detay
                        </a>
                    </td>
                </tr>
            `).join('');
        }
    } catch (error) {
        console.error('Error loading recent leaves:', error);
    }
}

// Fetch and display upcoming events
async function loadUpcomingEvents() {
    try {
        const response = await fetch('/api/Dashboard/upcoming-events');
        if (!response.ok) return;

        const events = await response.json();
        const container = document.getElementById('upcomingEventsContainer');

        if (container) {
            if (events.length === 0) {
                container.innerHTML = '<p class="text-muted text-center">Yaklaşan etkinlik bulunmuyor</p>';
                return;
            }

            container.innerHTML = events.map(evt => {
                const date = new Date(evt.startTime);
                return `
                <div class="event-item mb-3" data-event-id="${evt.eventId}">
                    <div class="d-flex">
                        <div class="event-date me-3">
                            <div class="text-center">
                                <div class="day">${date.getDate()}</div>
                                <div class="month">${date.toLocaleString('tr-TR', { month: 'short' })}</div>
                            </div>
                        </div>
                        <div class="flex-grow-1">
                            <h6 class="mb-1">${evt.title}</h6>
                            <p class="text-muted mb-1">
                                <i class="bi bi-clock me-1"></i>
                                ${date.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' })}
                            </p>
                            <p class="text-muted mb-0">
                                <i class="bi bi-geo-alt me-1"></i>${evt.location || 'Konum belirtilmedi'}
                            </p>
                        </div>
                        <div>
                            <a href="/Event/Detail/${evt.eventId}" class="btn btn-sm btn-outline-primary">
                                Detay
                            </a>
                        </div>
                    </div>
                </div>`;
            }).join('');
        }
    } catch (error) {
        console.error('Error loading upcoming events:', error);
    }
}

// Fetch and display recent notifications
async function loadRecentNotifications() {
    try {
        const response = await fetch('/api/Dashboard/recent-notifications');
        if (!response.ok) return;

        const notifications = await response.json();
        const container = document.getElementById('recentNotificationsContainer');

        if (container) {
            if (notifications.length === 0) {
                container.innerHTML = '<p class="text-muted text-center">Bildirim bulunmuyor</p>';
                return;
            }

            container.innerHTML = notifications.map(n => `
                <div class="notification-item ${n.isRead ? '' : 'unread'}">
                    <div class="notification-icon bg-${n.typeColor || 'primary'}">
                        <i class="bi bi-${n.typeIcon || 'bell'}"></i>
                    </div>
                    <div class="notification-content">
                        <p class="mb-1">${n.message}</p>
                        <small class="text-muted">${new Date(n.sendDate).toLocaleString('tr-TR')}</small>
                    </div>
                </div>
            `).join('');
        }
    } catch (error) {
        console.error('Error loading notifications:', error);
    }
}

// Fetch HR Stats
async function loadHRStats() {
    try {
        const response = await fetch('/api/Dashboard/hr-stats');
        if (!response.ok) return;

        const data = await response.json();

        // Update Department Chart
        if (data.departmentDistribution) {
            const labels = Object.keys(data.departmentDistribution);
            const values = Object.values(data.departmentDistribution);
            initializeDepartmentChart(labels, values);
        }

    } catch (error) {
        console.error('Error loading HR stats:', error);
    }
}

// Helper to get status color
function getStatusColor(status) {
    switch (status) {
        case 'Approved': return 'success';
        case 'Pending': return 'warning';
        case 'Rejected': return 'danger';
        default: return 'secondary';
    }
}

// Initialize Leave Status Chart
function initializeLeaveStatusChart(approved, pending, rejected) {
    const canvas = document.getElementById('leaveStatusChart');
    if (!canvas) return;

    if (leaveStatusChart) {
        leaveStatusChart.destroy();
    }

    const ctx = canvas.getContext('2d');
    leaveStatusChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: ['Onaylanan', 'Bekleyen', 'Reddedilen'],
            datasets: [{
                data: [approved, pending, rejected],
                backgroundColor: ['#28a745', '#ffc107', '#dc3545'],
                borderWidth: 0
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: { padding: 20, font: { size: 12 } }
                }
            }
        }
    });
}

// Initialize Department Chart
function initializeDepartmentChart(labels, data) {
    const canvas = document.getElementById('departmentChart');
    if (!canvas) return;

    if (departmentChart) {
        departmentChart.destroy();
    }

    const ctx = canvas.getContext('2d');
    departmentChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Çalışan Sayısı',
                data: data,
                backgroundColor: 'rgba(102, 126, 234, 0.8)',
                borderColor: 'rgba(102, 126, 234, 1)',
                borderWidth: 2,
                borderRadius: 8
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            scales: {
                y: { beginAtZero: true, ticks: { stepSize: 1 } }
            }
        }
    });
}

// Notification polling
let notificationInterval = null;

function startNotificationPolling() {
    notificationInterval = setInterval(fetchNotifications, 30000);
}

function stopNotificationPolling() {
    if (notificationInterval) {
        clearInterval(notificationInterval);
        notificationInterval = null;
    }
}

async function fetchNotifications() {
    try {
        const response = await fetch('/api/Dashboard/stats'); // Reusing stats endpoint which includes unread count
        if (response.ok) {
            const data = await response.json();
            updateNotificationBadge(data.unreadNotifications);
        }
    } catch (error) {
        console.error('Error fetching notifications:', error);
    }
}

function updateNotificationBadge(count) {
    const badge = document.querySelector('.notification-badge');
    if (badge) {
        if (count > 0) {
            badge.textContent = count > 99 ? '99+' : count;
            badge.style.display = 'block';
        } else {
            badge.style.display = 'none';
        }
    }
}

// Widget refresh
function initializeWidgetRefresh() {
    const refreshButtons = document.querySelectorAll('[data-widget-refresh]');
    refreshButtons.forEach(button => {
        button.addEventListener('click', function () {
            const widgetId = this.dataset.widgetRefresh;
            refreshWidget(widgetId);
        });
    });
}

async function refreshWidget(widgetId) {
    const widget = document.getElementById(widgetId);
    if (!widget) return;

    widget.classList.add('loading');

    try {
        // Reload specific data based on widget ID
        if (widgetId === 'statsWidget') await loadStats();
        else if (widgetId === 'leavesWidget') await loadRecentLeaves();
        else if (widgetId === 'eventsWidget') await loadUpcomingEvents();
        else if (widgetId === 'notificationsWidget') await loadRecentNotifications();

        // console.log('Widget refreshed:', widgetId);
    } catch (error) {
        console.error('Error refreshing widget:', error);
    } finally {
        widget.classList.remove('loading');
    }
}

// Export functions for external use
window.dashboardFunctions = {
    refreshWidget,
    updateNotificationBadge,
    stopNotificationPolling,
    startNotificationPolling
};

// Cleanup on page unload
window.addEventListener('beforeunload', function () {
    stopNotificationPolling();
    if (leaveStatusChart) leaveStatusChart.destroy();
    if (departmentChart) departmentChart.destroy();
    if (salaryTrendChart) salaryTrendChart.destroy();
});

