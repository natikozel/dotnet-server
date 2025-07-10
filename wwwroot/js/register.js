// Enhanced Registration Form JavaScript
document.addEventListener('DOMContentLoaded', function() {
    const form = document.querySelector('form[asp-page="Register"]');
    const submitBtn = document.getElementById('submitBtn');
    const inputs = form.querySelectorAll('input, select');
    const phoneInput = document.getElementById('phoneInput');
    
    // Add floating label effect
    inputs.forEach(input => {
        input.addEventListener('focus', function() {
            this.parentElement.classList.add('focused');
        });
        
        input.addEventListener('blur', function() {
            if (!this.value) {
                this.parentElement.classList.remove('focused');
            }
        });
        
        // Check if input has value on page load
        if (input.value) {
            input.parentElement.classList.add('focused');
        }
    });
    
    // Phone number formatting
    if (phoneInput) {
        phoneInput.addEventListener('input', function(e) {
            let value = e.target.value.replace(/\D/g, ''); // Remove all non-digits
            
            // Limit to 10 digits maximum
            if (value.length > 10) {
                value = value.substring(0, 10);
            }
            
            // Format as 052-1234567
            if (value.length >= 4) {
                value = value.substring(0, 3) + '-' + value.substring(3);
            }
            
            e.target.value = value;
        });
        
        phoneInput.addEventListener('keydown', function(e) {
            // Allow backspace, delete, tab, escape, enter
            if ([8, 9, 27, 13, 46].indexOf(e.keyCode) !== -1 ||
                // Allow Ctrl+A, Ctrl+C, Ctrl+V, Ctrl+X
                (e.keyCode === 65 && e.ctrlKey === true) ||
                (e.keyCode === 67 && e.ctrlKey === true) ||
                (e.keyCode === 86 && e.ctrlKey === true) ||
                (e.keyCode === 88 && e.ctrlKey === true)) {
                return;
            }
            // Ensure that it is a number and stop the keypress
            if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
                e.preventDefault();
            }
        });
    }
    
    // Enhanced form validation
    inputs.forEach(input => {
        input.addEventListener('input', function() {
            validateField(this);
        });
    });
    
    function validateField(field) {
        const value = field.value.trim();
        const fieldType = field.type;
        const fieldName = field.name;
        
        // Remove existing validation classes
        field.classList.remove('is-valid', 'is-invalid');
        
        // Basic validation
        if (fieldName === 'Player.FirstName') {
            if (value.length >= 2) {
                field.classList.add('is-valid');
            } else if (value.length > 0) {
                field.classList.add('is-invalid');
            }
        } else if (fieldName === 'Player.PlayerId') {
            const playerId = parseInt(value);
            if (playerId >= 1 && playerId <= 1000) {
                field.classList.add('is-valid');
            } else if (value.length > 0) {
                field.classList.add('is-invalid');
            }
        } else if (fieldName === 'Player.PhoneNumber') {
            const phoneRegex = /^0\d{2}-\d{7}$/;
            if (phoneRegex.test(value)) {
                field.classList.add('is-valid');
            } else if (value.length > 0) {
                field.classList.add('is-invalid');
            }
        } else if (fieldName === 'Player.Country') {
            if (value !== '') {
                field.classList.add('is-valid');
            }
        }
    }
    
    // Form submission with loading state
    form.addEventListener('submit', function(e) {
        // Check if form is valid before showing loading
        if (form.checkValidity()) {
            submitBtn.classList.add('loading');
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Creating Account...';
            submitBtn.disabled = true;
            
            // Form will submit normally - don't prevent default
            return true;
        }
        
        // If form is invalid, prevent submission
        e.preventDefault();
        return false;
    });
    
    // Add smooth scrolling to form on validation errors
    const validationErrors = document.querySelectorAll('.text-danger:not(:empty)');
    if (validationErrors.length > 0) {
        validationErrors[0].scrollIntoView({ 
            behavior: 'smooth', 
            block: 'center' 
        });
    }
    
    // Add some interactive animations
    const infoItems = document.querySelectorAll('.info-item');
    infoItems.forEach((item, index) => {
        item.style.animationDelay = `${index * 0.1}s`;
        item.classList.add('animate-fadeInUp');
    });
    
    // Add success/error message animations
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        alert.style.animation = 'slideInUp 0.5s ease-out';
    });
});

// Add some keyframe animations for the new effects
const style = document.createElement('style');
style.textContent = `
    @keyframes fadeOut {
        from { opacity: 1; transform: translateY(0); }
        to { opacity: 0; transform: translateY(-20px); }
    }
`;
document.head.appendChild(style); 