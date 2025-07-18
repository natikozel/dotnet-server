
const countryCodeMap = {
    "Afghanistan": "AF",
    "Albania": "AL",
    "Algeria": "DZ",
    "Andorra": "AD",
    "Angola": "AO",
    "Antigua and Barbuda": "AG",
    "Argentina": "AR",
    "Armenia": "AM",
    "Australia": "AU",
    "Austria": "AT",
    "Azerbaijan": "AZ",
    "Bahamas": "BS",
    "Bahrain": "BH",
    "Bangladesh": "BD",
    "Barbados": "BB",
    "Belarus": "BY",
    "Belgium": "BE",
    "Belize": "BZ",
    "Benin": "BJ",
    "Bhutan": "BT",
    "Bolivia": "BO",
    "Bosnia and Herzegovina": "BA",
    "Botswana": "BW",
    "Brazil": "BR",
    "Brunei": "BN",
    "Bulgaria": "BG",
    "Burkina Faso": "BF",
    "Burundi": "BI",
    "Cabo Verde": "CV",
    "Cambodia": "KH",
    "Cameroon": "CM",
    "Canada": "CA",
    "Central African Republic": "CF",
    "Chad": "TD",
    "Chile": "CL",
    "China": "CN",
    "Colombia": "CO",
    "Comoros": "KM",
    "Congo (Congo-Brazzaville)": "CG",
    "Costa Rica": "CR",
    "Croatia": "HR",
    "Cuba": "CU",
    "Cyprus": "CY",
    "Czechia (Czech Republic)": "CZ",
    "Democratic Republic of the Congo": "CD",
    "Denmark": "DK",
    "Djibouti": "DJ",
    "Dominica": "DM",
    "Dominican Republic": "DO",
    "Ecuador": "EC",
    "Egypt": "EG",
    "El Salvador": "SV",
    "Equatorial Guinea": "GQ",
    "Eritrea": "ER",
    "Estonia": "EE",
    "Eswatini (fmr. 'Swaziland')": "SZ",
    "Ethiopia": "ET",
    "Fiji": "FJ",
    "Finland": "FI",
    "France": "FR",
    "Gabon": "GA",
    "Gambia": "GM",
    "Georgia": "GE",
    "Germany": "DE",
    "Ghana": "GH",
    "Greece": "GR",
    "Grenada": "GD",
    "Guatemala": "GT",
    "Guinea": "GN",
    "Guinea-Bissau": "GW",
    "Guyana": "GY",
    "Haiti": "HT",
    "Honduras": "HN",
    "Hungary": "HU",
    "Iceland": "IS",
    "India": "IN",
    "Indonesia": "ID",
    "Iran": "IR",
    "Iraq": "IQ",
    "Ireland": "IE",
    "Israel": "IL",
    "Italy": "IT",
    "Jamaica": "JM",
    "Japan": "JP",
    "Jordan": "JO",
    "Kazakhstan": "KZ",
    "Kenya": "KE",
    "Kiribati": "KI",
    "Kuwait": "KW",
    "Kyrgyzstan": "KG",
    "Laos": "LA",
    "Latvia": "LV",
    "Lebanon": "LB",
    "Lesotho": "LS",
    "Liberia": "LR",
    "Libya": "LY",
    "Liechtenstein": "LI",
    "Lithuania": "LT",
    "Luxembourg": "LU",
    "Madagascar": "MG",
    "Malawi": "MW",
    "Malaysia": "MY",
    "Maldives": "MV",
    "Mali": "ML",
    "Malta": "MT",
    "Marshall Islands": "MH",
    "Mauritania": "MR",
    "Mauritius": "MU",
    "Mexico": "MX",
    "Micronesia": "FM",
    "Moldova": "MD",
    "Monaco": "MC",
    "Mongolia": "MN",
    "Montenegro": "ME",
    "Morocco": "MA",
    "Mozambique": "MZ",
    "Myanmar (formerly Burma)": "MM",
    "Namibia": "NA",
    "Nauru": "NR",
    "Nepal": "NP",
    "Netherlands": "NL",
    "New Zealand": "NZ",
    "Nicaragua": "NI",
    "Niger": "NE",
    "Nigeria": "NG",
    "North Korea": "KP",
    "North Macedonia": "MK",
    "Norway": "NO",
    "Oman": "OM",
    "Pakistan": "PK",
    "Palau": "PW",
    "Panama": "PA",
    "Papua New Guinea": "PG",
    "Paraguay": "PY",
    "Peru": "PE",
    "Philippines": "PH",
    "Poland": "PL",
    "Portugal": "PT",
    "Qatar": "QA",
    "Romania": "RO",
    "Russia": "RU",
    "Rwanda": "RW",
    "Saint Kitts and Nevis": "KN",
    "Saint Lucia": "LC",
    "Saint Vincent and the Grenadines": "VC",
    "Samoa": "WS",
    "San Marino": "SM",
    "Sao Tome and Principe": "ST",
    "Saudi Arabia": "SA",
    "Senegal": "SN",
    "Serbia": "RS",
    "Seychelles": "SC",
    "Sierra Leone": "SL",
    "Singapore": "SG",
    "Slovakia": "SK",
    "Slovenia": "SI",
    "Solomon Islands": "SB",
    "Somalia": "SO",
    "South Africa": "ZA",
    "South Korea": "KR",
    "South Sudan": "SS",
    "Spain": "ES",
    "Sri Lanka": "LK",
    "Sudan": "SD",
    "Suriname": "SR",
    "Sweden": "SE",
    "Switzerland": "CH",
    "Syria": "SY",
    "Taiwan": "TW",
    "Tajikistan": "TJ",
    "Tanzania": "TZ",
    "Thailand": "TH",
    "Timor-Leste": "TL",
    "Togo": "TG",
    "Tonga": "TO",
    "Trinidad and Tobago": "TT",
    "Tunisia": "TN",
    "Turkey": "TR",
    "Turkmenistan": "TM",
    "Tuvalu": "TV",
    "Uganda": "UG",
    "Ukraine": "UA",
    "United Arab Emirates": "AE",
    "United Kingdom": "GB",
    "United States": "US",
    "Uruguay": "UY",
    "Uzbekistan": "UZ",
    "Vanuatu": "VU",
    "Vatican City": "VA",
    "Venezuela": "VE",
    "Vietnam": "VN",
    "Yemen": "YE",
    "Zambia": "ZM",
    "Zimbabwe": "ZW"
};

function validatePhoneNumber() {
    var phoneInput = document.getElementById('phoneInput').value;
    var phoneError = document.getElementById('phoneError');
    var selectedCountry = document.getElementById('countryField').value;

    // Get the country code based on the selected country
    var countryCode = countryCodeMap[selectedCountry];

    // If no country code is found, skip phone number validation and allow form submission
    if (!countryCode) {
        phoneError.textContent = ''; // Clear any error message
        return true; // Allow form submission
    }

    // Check if the phone number is 10 digits long
    if (phoneInput.length !== 10) {
        phoneError.textContent = 'Phone number must be exactly 10 digits.';
        return false; // Prevent form submission
    }

    try {
        // Parse the phone number using libphonenumber-js
        var phoneNumber = libphonenumber.parsePhoneNumber(phoneInput, countryCode);
        if (!phoneNumber.isValid()) {
            phoneError.textContent = 'Phone number is not valid for ' + selectedCountry;
            return false; // Prevent form submission
        }
        phoneError.textContent = ''; // Clear error if valid
        return true; // Allow form submission
    } catch (error) {
        phoneError.textContent = 'Phone number is not valid for ' + selectedCountry;
        return false; // Prevent form submission
    }
}


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