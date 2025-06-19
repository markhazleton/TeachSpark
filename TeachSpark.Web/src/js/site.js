// Import Bootstrap JavaScript
const bootstrap = require('bootstrap');

// Main application JavaScript
(function () {
    'use strict';

    // Initialize when DOM is loaded
    document.addEventListener('DOMContentLoaded', function () {
        console.log('TeachSpark.Web application loaded');

        // Initialize Bootstrap tooltips
        initializeTooltips();

        // Initialize custom functionality
        initializeCustomFeatures();
    });

    // Initialize Bootstrap tooltips
    function initializeTooltips() {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }

    // Initialize custom application features
    function initializeCustomFeatures() {
        // Add fade-in animation to main content
        const mainContent = document.querySelector('main');
        if (mainContent) {
            mainContent.classList.add('fade-in');
        }

        // Handle navigation active states
        setActiveNavigation();

        // Add click handlers for custom buttons
        addCustomEventHandlers();
    }

    // Set active navigation item based on current page
    function setActiveNavigation() {
        const currentPath = window.location.pathname;
        const navLinks = document.querySelectorAll('.navbar-nav .nav-link');

        navLinks.forEach(link => {
            link.classList.remove('active');
            if (link.getAttribute('href') === currentPath) {
                link.classList.add('active');
            }
        });
    }

    // Add custom event handlers
    function addCustomEventHandlers() {
        // Example: Handle custom button clicks
        const customButtons = document.querySelectorAll('.btn-primary-custom');
        customButtons.forEach(button => {
            button.addEventListener('click', function (_e) {
                console.log('Custom button clicked:', this);
                // Add your custom logic here
            });
        });

        // Example: Handle form submissions with loading states
        const forms = document.querySelectorAll('form');
        forms.forEach(form => {
            form.addEventListener('submit', function (_e) {
                const submitButton = this.querySelector('button[type="submit"], input[type="submit"]');
                if (submitButton) {
                    submitButton.disabled = true;
                    submitButton.innerHTML =
                        '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Loading...';
                }
            });
        });
    }

    // Utility functions
    window.TeachSpark = {
        // Show a Bootstrap alert
        showAlert: function (message, type = 'info') {
            const alertContainer = document.querySelector('#alert-container') || document.body;
            const alert = document.createElement('div');
            alert.className = `alert alert-${type} alert-dismissible fade show`;
            alert.innerHTML = `
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            `;
            alertContainer.insertBefore(alert, alertContainer.firstChild);

            // Auto-hide after 5 seconds
            setTimeout(() => {
                if (alert.parentNode) {
                    alert.remove();
                }
            }, 5000);
        },

        // Smooth scroll to element
        scrollTo: function (elementId) {
            const element = document.getElementById(elementId);
            if (element) {
                element.scrollIntoView({ behavior: 'smooth' });
            }
        },
    };
})();
