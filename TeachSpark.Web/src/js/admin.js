// Admin Dashboard Enhancement JavaScript
// Common functionality for all admin pages

document.addEventListener('DOMContentLoaded', function() {
    // Initialize tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Auto-hide alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
    alerts.forEach(function(alert) {
        setTimeout(function() {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    });

    // Confirmation dialogs for delete actions
    const deleteButtons = document.querySelectorAll('a[href*="/Delete"], button[type="submit"][class*="btn-danger"]');
    deleteButtons.forEach(function(button) {
        button.addEventListener('click', function(e) {
            if (!button.hasAttribute('data-no-confirm')) {
                const confirmed = window.confirm('Are you sure you want to delete this item? This action cannot be undone.');
                if (!confirmed) {
                    e.preventDefault();
                    return false;
                }
            }
        });
    });

    // Enhanced search functionality for DataTables
    if (typeof $ !== 'undefined' && $.fn.DataTable) {
        // Add custom search delay
        $.fn.dataTable.ext.search.push(function(_settings, _searchData, _index, _rowData, _counter) {
            return true; // Default: show all rows
        });
    }

    // Add loading states to buttons
    const submitButtons = document.querySelectorAll('button[type="submit"]:not([data-no-loading])');
    submitButtons.forEach(function(button) {
        button.addEventListener('click', function() {
            setTimeout(function() {
                button.disabled = true;
                button.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Processing...';
            }, 100);
        });
    });

    // Auto-refresh for real-time data (dashboard)
    if (window.location.pathname.includes('/Admin/Dashboard')) {
        const refreshInterval = 5 * 60 * 1000; // 5 minutes
        setTimeout(function() {
            if (window.confirm('Refresh dashboard data?')) {
                window.location.reload();
            }
        }, refreshInterval);
    }
});

// Utility functions for admin pages
window.AdminUtils = {
    // Format numbers with commas
    formatNumber: function(num) {
        return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',');
    },

    // Show success message
    showSuccess: function(message) {
        this.showAlert(message, 'success');
    },

    // Show error message
    showError: function(message) {
        this.showAlert(message, 'danger');
    },

    // Show info message
    showInfo: function(message) {
        this.showAlert(message, 'info');
    },

    // Generic alert display
    showAlert: function(message, type) {
        const alertHtml = `
            <div class="alert alert-${type} alert-dismissible fade show" role="alert">
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        `;
        
        const container = document.querySelector('.container-fluid') || document.querySelector('.container');
        if (container) {
            container.insertAdjacentHTML('afterbegin', alertHtml);
        }
    },

    // Confirm dialog with custom message
    confirm: function(message, callback) {
        if (window.confirm(message)) {
            callback();
        }
    },

    // Export table data
    exportTable: function(tableId) {
        if (typeof $ !== 'undefined' && $.fn.DataTable) {
            const table = $(tableId).DataTable();
            table.button('.buttons-excel').trigger();
        }
    }
};
