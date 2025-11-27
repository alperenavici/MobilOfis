// Event Management JavaScript

// Initialize on page load
document.addEventListener('DOMContentLoaded', function () {
    initializeViewModeSwitch();
    initializeCalendar();
    initializeEventFilters();
    initializeEventForms();
});

// View mode switching
let currentViewMode = 'calendar'; // list, calendar, card

function initializeViewModeSwitch() {
    const viewButtons = document.querySelectorAll('[data-view-mode]');

    viewButtons.forEach(btn => {
        btn.addEventListener('click', function () {
            const mode = this.dataset.viewMode;
            switchViewMode(mode);

            // Update active button
            viewButtons.forEach(b => b.classList.remove('active'));
            this.classList.add('active');
        });
    });

    // Set initial view mode from URL or default
    const urlParams = new URLSearchParams(window.location.search);
    const initialMode = urlParams.get('view') || 'calendar';
    switchViewMode(initialMode);
}

function switchViewMode(mode) {
    currentViewMode = mode;

    // Hide all views
    document.querySelectorAll('.event-view').forEach(view => {
        view.style.display = 'none';
    });

    // Show selected view
    const selectedView = document.getElementById(`${mode}View`);
    if (selectedView) {
        selectedView.style.display = 'block';
    }

    // Initialize calendar if switching to calendar view
    if (mode === 'calendar') {
        initializeCalendar();
    }
}

// Calendar initialization (FullCalendar)
let calendar = null;

function initializeCalendar() {
    const calendarEl = document.getElementById('eventCalendar');

    if (!calendarEl) return;
    
    // Destroy existing calendar if it exists
    if (calendar) {
        calendar.destroy();
        calendar = null;
    }

    // Check if mobile
    const isMobile = window.innerWidth < 768;
    
    // Mobile configuration
    const mobileConfig = {
        initialView: 'listWeek', // Mobilde liste görünümü daha uygun
        headerToolbar: {
            left: 'prev,next',
            center: 'title',
            right: ''
        },
        height: 'auto',
        aspectRatio: 1.2
    };
    
    // Desktop configuration
    const desktopConfig = {
        initialView: 'dayGridMonth',
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,timeGridWeek,listWeek'
        },
        height: 'auto'
    };

    const config = isMobile ? mobileConfig : desktopConfig;

    calendar = new FullCalendar.Calendar(calendarEl, {
        ...config,
        locale: 'tr',
        buttonText: {
            today: 'Bugün',
            month: 'Ay',
            week: 'Hafta',
            list: 'Liste'
        },
        views: {
            dayGridMonth: {
                dayHeaderFormat: { weekday: 'short' },
                dayMaxEvents: isMobile ? 2 : 3,
                moreLinkClick: 'popover'
            },
            timeGridWeek: {
                dayHeaderFormat: { weekday: 'short', day: 'numeric' },
                slotMinTime: '08:00:00',
                slotMaxTime: '20:00:00',
                allDaySlot: false
            },
            listWeek: {
                listDayFormat: { weekday: 'short', day: 'numeric', month: 'short' },
                listDaySideFormat: false
            }
        },
        events: async function (info, successCallback, failureCallback) {
            try {
                const response = await fetch('/api/Event/calendar');
                const events = await response.json();

                const formattedEvents = events.map(event => ({
                    id: event.id,
                    title: event.title,
                    start: event.startDate,
                    end: event.endDate,
                    backgroundColor: getEventColor(event.type),
                    borderColor: getEventColor(event.type)
                }));

                successCallback(formattedEvents);
            } catch (error) {
                console.error('Error loading events:', error);
                failureCallback(error);
            }
        },
        eventClick: function (info) {
            window.location.href = `/Event/Detail/${info.event.id}`;
        },
        eventMouseEnter: function (info) {
            info.el.style.cursor = 'pointer';
        }
    });

    calendar.render();
    
    // Update size after render
    setTimeout(() => {
        calendar.updateSize();
    }, 100);
    
    // Handle window resize for mobile/desktop switching
    let resizeTimeout;
    window.addEventListener('resize', function() {
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(function() {
            if (calendar) {
                const wasMobile = window.innerWidth < 768;
                const isMobile = window.innerWidth < 768;
                
                // Only reinitialize if switching between mobile/desktop
                if (wasMobile !== isMobile) {
                    initializeCalendar();
                } else {
                    calendar.updateSize();
                }
            }
        }, 250);
    });
}

function getEventColor(eventType) {
    const colors = {
        'Meeting': '#667eea',
        'Training': '#f093fb',
        'Social': '#a1c4fd',
        'Other': '#fbc2eb'
    };
    return colors[eventType] || '#667eea';
}

// Event filters
function initializeEventFilters() {
    const filterForm = document.getElementById('eventFilterForm');

    if (filterForm) {
        filterForm.addEventListener('submit', function (e) {
            e.preventDefault();
            applyEventFilters();
        });
    }
}

function applyEventFilters() {
    const type = document.getElementById('filterType')?.value;
    const startDate = document.getElementById('filterStartDate')?.value;
    const endDate = document.getElementById('filterEndDate')?.value;

    const params = new URLSearchParams();
    if (type) params.append('type', type);
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);

    window.location.href = `/Event/Index?${params.toString()}`;
}

function clearEventFilters() {
    window.location.href = '/Event/Index';
}

// Event form handling
function initializeEventForms() {
    const eventForm = document.getElementById('eventForm');

    if (eventForm) {
        eventForm.addEventListener('submit', handleEventSubmit);
    }

    // Banner image preview
    const bannerInput = document.getElementById('eventBanner');
    if (bannerInput) {
        bannerInput.addEventListener('change', previewBannerImage);
    }
}

function previewBannerImage(e) {
    const file = e.target.files[0];
    const preview = document.getElementById('bannerPreview');

    if (file && preview) {
        // Validate file size (max 2MB)
        if (file.size > 2 * 1024 * 1024) {
            showToast('Dosya boyutu 2MB\'dan küçük olmalıdır', 'error');
            e.target.value = '';
            return;
        }

        // Validate file type
        if (!file.type.startsWith('image/')) {
            showToast('Sadece resim dosyaları yüklenebilir', 'error');
            e.target.value = '';
            return;
        }

        const reader = new FileReader();
        reader.onload = function (e) {
            preview.innerHTML = `
                <img src="${e.target.result}" alt="Banner Preview" class="img-fluid rounded">
                <button type="button" class="btn btn-sm btn-danger mt-2" onclick="clearBannerPreview()">
                    Kaldır
                </button>
            `;
        };
        reader.readAsDataURL(file);
    }
}

function clearBannerPreview() {
    const bannerInput = document.getElementById('eventBanner');
    const preview = document.getElementById('bannerPreview');

    if (bannerInput) bannerInput.value = '';
    if (preview) preview.innerHTML = '';
}

async function handleEventSubmit(e) {
    e.preventDefault();

    const form = e.target;
    const formData = new FormData(form);

    // Validate dates
    const startDate = new Date(formData.get('startDate'));
    const endDate = new Date(formData.get('endDate'));

    if (endDate < startDate) {
        showToast('Bitiş tarihi başlangıç tarihinden önce olamaz', 'error');
        return;
    }

    // Convert FormData to JSON for API
    const data = {
        title: formData.get('Title'),
        description: formData.get('Description'),
        startTime: formData.get('StartDate'),
        endTime: formData.get('EndDate'),
        location: formData.get('Location'),
        participantIds: [] // Default empty or handle selection if UI exists
    };

    // Show loading
    const submitBtn = form.querySelector('button[type="submit"]');
    const originalText = submitBtn.innerHTML;
    submitBtn.disabled = true;
    submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Kaydediliyor...';

    try {
        const response = await fetch('/api/Event', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        });

        if (response.ok) {
            showToast('Etkinlik başarıyla kaydedildi', 'success');
            setTimeout(() => {
                window.location.href = '/Event/Index';
            }, 1500);
        } else {
            const errorData = await response.json();
            showToast(errorData.message || 'Etkinlik kaydedilemedi', 'error');
        }
    } catch (error) {
        console.error('Error:', error);
        showToast('Bir hata oluştu', 'error');
    } finally {
        submitBtn.disabled = false;
        submitBtn.innerHTML = originalText;
    }
}

// Join/Leave event
async function joinEvent(eventId) {
    if (!confirm('Bu etkinliğe katılmak istediğinizden emin misiniz?')) {
        return;
    }

    try {
        const response = await fetch(`/api/Event/join/${eventId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            showToast('Etkinliğe başarıyla katıldınız', 'success');
            setTimeout(() => window.location.reload(), 1500);
        } else {
            const errorData = await response.json();
            showToast(errorData.message || 'İşlem başarısız oldu', 'error');
        }
    } catch (error) {
        console.error('Error:', error);
        showToast('Bir hata oluştu', 'error');
    }
}

async function leaveEvent(eventId) {
    if (!confirm('Bu etkinlikten ayrılmak istediğinizden emin misiniz?')) {
        return;
    }

    try {
        const response = await fetch(`/api/Event/leave/${eventId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            showToast('Etkinlikten ayrıldınız', 'success');
            setTimeout(() => window.location.reload(), 1500);
        } else {
            const errorData = await response.json();
            showToast(errorData.message || 'İşlem başarısız oldu', 'error');
        }
    } catch (error) {
        console.error('Error:', error);
        showToast('Bir hata oluştu', 'error');
    }
}

async function deleteEvent(eventId) {
    if (!confirm('Bu etkinliği silmek istediğinizden emin misiniz? Bu işlem geri alınamaz.')) {
        return;
    }

    try {
        const response = await fetch(`/api/Event/${eventId}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            showToast('Etkinlik silindi', 'success');
            setTimeout(() => {
                window.location.href = '/Event/Index';
            }, 1500);
        } else {
            const errorData = await response.json();
            showToast(errorData.message || 'İşlem başarısız oldu', 'error');
        }
    } catch (error) {
        console.error('Error:', error);
        showToast('Bir hata oluştu', 'error');
    }
}

// Export to calendar (iCal format)
function exportToCalendar(eventId) {
    window.location.href = `/Event/ExportToCalendar/${eventId}`;
}

// Export functions
window.eventFunctions = {
    switchViewMode,
    joinEvent,
    leaveEvent,
    deleteEvent,
    exportToCalendar,
    clearBannerPreview,
    clearEventFilters
};

