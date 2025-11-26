// Leave Management JavaScript

// Initialize on page load
document.addEventListener('DOMContentLoaded', function () {
    initializeLeaveForms();
    initializeDatePickers();
    initializeFileUpload();
    initializeFilters();
});

// Initialize leave forms
function initializeLeaveForms() {
    const leaveForm = document.getElementById('createLeaveForm');
    if (leaveForm) {
        leaveForm.addEventListener('submit', handleLeaveSubmit);

        // Date change listeners
        const startDate = document.getElementById('startDate');
        const endDate = document.getElementById('endDate');

        if (startDate && endDate) {
            startDate.addEventListener('change', calculateLeaveDays);
            endDate.addEventListener('change', calculateLeaveDays);
        }
    }
}

// Calculate business days between dates
function calculateLeaveDays() {
    const startDate = document.getElementById('startDate');
    const endDate = document.getElementById('endDate');
    const daysDisplay = document.getElementById('leaveDaysDisplay');

    if (!startDate || !endDate || !startDate.value || !endDate.value) {
        if (daysDisplay) daysDisplay.textContent = '0 gün';
        return;
    }

    const start = new Date(startDate.value);
    const end = new Date(endDate.value);

    if (end < start) {
        if (daysDisplay) {
            daysDisplay.textContent = 'Geçersiz tarih aralığı';
            daysDisplay.classList.add('text-danger');
        }
        return;
    }

    const days = calculateBusinessDays(start, end);

    if (daysDisplay) {
        daysDisplay.textContent = `${days} gün`;
        daysDisplay.classList.remove('text-danger');
    }
}

// Calculate business days (excluding weekends)
function calculateBusinessDays(startDate, endDate) {
    let count = 0;
    let current = new Date(startDate);

    while (current <= endDate) {
        const dayOfWeek = current.getDay();
        if (dayOfWeek !== 0 && dayOfWeek !== 6) { // Not Sunday (0) or Saturday (6)
            count++;
        }
        current.setDate(current.getDate() + 1);
    }

    return count;
}

// Initialize date pickers
function initializeDatePickers() {
    const dateInputs = document.querySelectorAll('input[type="date"]');

    dateInputs.forEach(input => {
        // Set min date to today
        if (input.classList.contains('future-only')) {
            const today = new Date().toISOString().split('T')[0];
            input.min = today;
        }
    });
}

// File upload handling
function initializeFileUpload() {
    const fileInput = document.getElementById('leaveDocument');
    const filePreview = document.getElementById('filePreview');

    if (fileInput) {
        fileInput.addEventListener('change', function (e) {
            const file = e.target.files[0];

            if (file) {
                // Validate file size (max 5MB)
                if (file.size > 5 * 1024 * 1024) {
                    showToast('Dosya boyutu 5MB\'dan küçük olmalıdır', 'error');
                    fileInput.value = '';
                    return;
                }

                // Validate file type
                const allowedTypes = ['application/pdf', 'image/jpeg', 'image/png', 'image/jpg'];
                if (!allowedTypes.includes(file.type)) {
                    showToast('Sadece PDF, JPG ve PNG dosyaları yüklenebilir', 'error');
                    fileInput.value = '';
                    return;
                }

                // Show preview
                if (filePreview) {
                    filePreview.innerHTML = `
                        <div class="file-preview-item">
                            <i class="bi bi-file-earmark-text"></i>
                            <span>${file.name}</span>
                            <button type="button" class="btn btn-sm btn-danger" onclick="clearFileUpload()">
                                <i class="bi bi-x"></i>
                            </button>
                        </div>
                    `;
                    filePreview.style.display = 'block';
                }
            }
        });
    }
}

function clearFileUpload() {
    const fileInput = document.getElementById('leaveDocument');
    const filePreview = document.getElementById('filePreview');

    if (fileInput) fileInput.value = '';
    if (filePreview) filePreview.style.display = 'none';
}

// Handle leave form submission
async function handleLeaveSubmit(e) {
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
        startDate: formData.get('StartDate'),
        endDate: formData.get('EndDate'),
        leavesType: formData.get('LeavesType'), // Enum value expected
        reason: formData.get('Reason')
    };

    // Show loading
    const submitBtn = form.querySelector('button[type="submit"]');
    const originalText = submitBtn.innerHTML;
    submitBtn.disabled = true;
    submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Gönderiliyor...';

    try {
        const response = await fetch('/api/Leave/request', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        });

        if (response.ok) {
            showToast('İzin talebi başarıyla oluşturuldu', 'success');

            // Close modal and refresh page
            const modal = bootstrap.Modal.getInstance(document.getElementById('createLeaveModal'));
            if (modal) modal.hide();

            setTimeout(() => {
                window.location.reload();
            }, 1500);
        } else {
            const errorData = await response.json();
            showToast(errorData.message || 'İzin talebi oluşturulamadı', 'error');
        }
    } catch (error) {
        console.error('Error:', error);
        showToast('Bir hata oluştu', 'error');
    } finally {
        submitBtn.disabled = false;
        submitBtn.innerHTML = originalText;
    }
}

// Filter handling
function initializeFilters() {
    const filterForm = document.getElementById('leaveFilterForm');

    if (filterForm) {
        filterForm.addEventListener('submit', function (e) {
            e.preventDefault();
            applyFilters();
        });
    }

    // Quick filter buttons
    const quickFilters = document.querySelectorAll('[data-filter]');
    quickFilters.forEach(btn => {
        btn.addEventListener('click', function () {
            const filterType = this.dataset.filter;
            applyQuickFilter(filterType);
        });
    });
}

function applyFilters() {
    const status = document.getElementById('Filters_Status')?.value;
    const type = document.getElementById('Filters_LeavesType')?.value;
    const year = document.getElementById('Filters_Year')?.value;

    const params = new URLSearchParams();
    if (status) params.append('status', status);
    if (type) params.append('leavesType', type);
    if (year) params.append('year', year);

    window.location.href = `/Leave/MyLeaves?${params.toString()}`;
}

function applyQuickFilter(filterType) {
    const params = new URLSearchParams();
    params.append('status', filterType);
    window.location.href = `/Leave/MyLeaves?${params.toString()}`;
}

function clearFilters() {
    window.location.href = '/Leave/MyLeaves';
}

// Leave approval/rejection
async function approveLeave(leaveId) {
    if (!confirm('Bu izin talebini onaylamak istediğinizden emin misiniz?')) {
        return;
    }

    try {
        const response = await fetch(`/api/Leave/approve-manager/${leaveId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            showToast('İzin talebi onaylandı', 'success');
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

async function rejectLeave(leaveId) {
    const reason = prompt('Reddetme sebebini giriniz:');

    if (!reason || reason.trim() === '') {
        showToast('Reddetme sebebi gereklidir', 'warning');
        return;
    }

    try {
        const response = await fetch(`/api/Leave/reject/${leaveId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ rejectionReason: reason })
        });

        if (response.ok) {
            showToast('İzin talebi reddedildi', 'success');
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

async function cancelLeave(leaveId) {
    if (!confirm('Bu izin talebini iptal etmek istediğinizden emin misiniz?')) {
        return;
    }

    try {
        const response = await fetch(`/api/Leave/cancel/${leaveId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            showToast('İzin talebi iptal edildi', 'success');
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

// Export functions
window.leaveFunctions = {
    calculateLeaveDays,
    approveLeave,
    rejectLeave,
    cancelLeave,
    clearFileUpload,
    clearFilters
};

