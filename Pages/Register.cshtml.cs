using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Connect4Server.Models;
using Connect4Server.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Connect4Server.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly Connect4Context _context;

        // Static readonly list to hold countries
        public readonly List<string> CountriesList = new List<string>
        {
            "Afghanistan", "Albania", "Algeria", "Andorra", "Angola", "Antigua and Barbuda", "Argentina", "Armenia", "Australia",
            "Austria", "Azerbaijan", "Bahamas", "Bahrain", "Bangladesh", "Barbados", "Belarus", "Belgium", "Belize", "Benin",
            "Bhutan", "Bolivia", "Bosnia and Herzegovina", "Botswana", "Brazil", "Brunei", "Bulgaria", "Burkina Faso", "Burundi",
            "Cabo Verde", "Cambodia", "Cameroon", "Canada", "Central African Republic", "Chad", "Chile", "China", "Colombia",
            "Comoros", "Congo (Congo-Brazzaville)", "Costa Rica", "Croatia", "Cuba", "Cyprus", "Czechia (Czech Republic)",
            "Democratic Republic of the Congo", "Denmark", "Djibouti", "Dominica", "Dominican Republic", "Ecuador", "Egypt",
            "El Salvador", "Equatorial Guinea", "Eritrea", "Estonia", "Eswatini (fmr. 'Swaziland')", "Ethiopia", "Fiji", "Finland",
            "France", "Gabon", "Gambia", "Georgia", "Germany", "Ghana", "Greece", "Grenada", "Guatemala", "Guinea", "Guinea-Bissau",
            "Guyana", "Haiti", "Honduras", "Hungary", "Iceland", "India", "Indonesia", "Iran", "Iraq", "Ireland", "Israel", "Italy",
            "Jamaica", "Japan", "Jordan", "Kazakhstan", "Kenya", "Kiribati", "Kuwait", "Kyrgyzstan", "Laos", "Latvia", "Lebanon",
            "Lesotho", "Liberia", "Libya", "Liechtenstein", "Lithuania", "Luxembourg", "Madagascar", "Malawi", "Malaysia", "Maldives",
            "Mali", "Malta", "Marshall Islands", "Mauritania", "Mauritius", "Mexico", "Micronesia", "Moldova", "Monaco", "Mongolia",
            "Montenegro", "Morocco", "Mozambique", "Myanmar (formerly Burma)", "Namibia", "Nauru", "Nepal", "Netherlands", "New Zealand",
            "Nicaragua", "Niger", "Nigeria", "North Korea", "North Macedonia", "Norway", "Oman", "Pakistan", "Palau", "Panama",
            "Papua New Guinea", "Paraguay", "Peru", "Philippines", "Poland", "Portugal", "Qatar", "Romania", "Russia", "Rwanda",
            "Saint Kitts and Nevis", "Saint Lucia", "Saint Vincent and the Grenadines", "Samoa", "San Marino", "Sao Tome and Principe",
            "Saudi Arabia", "Senegal", "Serbia", "Seychelles", "Sierra Leone", "Singapore", "Slovakia", "Slovenia", "Solomon Islands",
            "Somalia", "South Africa", "South Korea", "South Sudan", "Spain", "Sri Lanka", "Sudan", "Suriname", "Sweden", "Switzerland",
            "Syria", "Taiwan", "Tajikistan", "Tanzania", "Thailand", "Timor-Leste", "Togo", "Tonga", "Trinidad and Tobago", "Tunisia",
            "Turkey", "Turkmenistan", "Tuvalu", "Uganda", "Ukraine", "United Arab Emirates", "United Kingdom", "United States",
            "Uruguay", "Uzbekistan", "Vanuatu", "Vatican City", "Venezuela", "Vietnam", "Yemen", "Zambia", "Zimbabwe"
        };

        public RegisterModel(Connect4Context context)
        {
            _context = context;
        }

        [BindProperty]
        public Player Player { get; set; } = new Player();

        public string? Message { get; set; }
        public bool Success { get; set; }

        public void OnGet()
        {
            // Initialize empty player for the form
            Player = new Player();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Custom phone validation based on country
            ValidatePhoneNumber();

            if (!ModelState.IsValid)
            {
                // Return page with validation errors
                return Page();
            }

            try
            {
                // Check if Player ID already exists
                var existingPlayerById = await _context.Players
                    .FirstOrDefaultAsync(p => p.PlayerId == Player.PlayerId);

                if (existingPlayerById != null)
                {
                    ModelState.AddModelError("Player.PlayerId",
                        $"Player ID {Player.PlayerId} is already taken. Please choose a different ID.");
                    return Page();
                }

                // Check if phone number already exists
                var existingPlayerByPhone = await _context.Players
                    .FirstOrDefaultAsync(p => p.PhoneNumber == Player.PhoneNumber);

                if (existingPlayerByPhone != null)
                {
                    ModelState.AddModelError("Player.PhoneNumber",
                        "This phone number is already registered. Please use a different phone number.");
                    return Page();
                }

                // Set registration date
                Player.RegistrationDate = DateTime.Now;

                // Save to database
                _context.Players.Add(Player);
                await _context.SaveChangesAsync();

                // Success message
                Success = true;
                Message = $"Congratulations {Player.FirstName}! You have been successfully registered with Player ID {Player.PlayerId}. You can now download the game client and start playing Connect 4!";

                // Clear the form for next registration
                Player = new Player();
                
                return Page();
            }
            catch (Exception ex)
            {
                Success = false;
                Message = "An error occurred during registration. Please try again.";

                // Log the error (in production, use proper logging)
                Console.WriteLine($"Registration error: {ex.Message}");

                return Page();
            }
        }

        // AJAX handler for checking existing Player ID
        public async Task<IActionResult> OnGetCheckPlayerIdAsync(int playerId)
        {
            try
            {
                var existingPlayer = await _context.Players
                    .FirstOrDefaultAsync(p => p.PlayerId == playerId);

                return new JsonResult(new { exists = existingPlayer != null });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = ex.Message });
            }
        }

        // AJAX handler for checking existing First Name
        public async Task<IActionResult> OnGetCheckFirstNameAsync(string firstName)
        {
            try
            {
                var existingPlayer = await _context.Players
                    .FirstOrDefaultAsync(p => p.FirstName.ToLower() == firstName.ToLower());

                return new JsonResult(new { exists = existingPlayer != null });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = ex.Message });
            }
        }

        private void ValidatePhoneNumber()
        {
            if (string.IsNullOrEmpty(Player.Country) || string.IsNullOrEmpty(Player.PhoneNumber))
                return;

            // Mapping of country names to country codes for phone validation
            var countryCodeMap = new Dictionary<string, string>
            {
                {"Afghanistan", "AF"}, {"Albania", "AL"}, {"Algeria", "DZ"}, {"Andorra", "AD"}, {"Angola", "AO"},
                {"Antigua and Barbuda", "AG"}, {"Argentina", "AR"}, {"Armenia", "AM"}, {"Australia", "AU"}, {"Austria", "AT"},
                {"Azerbaijan", "AZ"}, {"Bahamas", "BS"}, {"Bahrain", "BH"}, {"Bangladesh", "BD"}, {"Barbados", "BB"},
                {"Belarus", "BY"}, {"Belgium", "BE"}, {"Belize", "BZ"}, {"Benin", "BJ"}, {"Bhutan", "BT"},
                {"Bolivia", "BO"}, {"Bosnia and Herzegovina", "BA"}, {"Botswana", "BW"}, {"Brazil", "BR"}, {"Brunei", "BN"},
                {"Bulgaria", "BG"}, {"Burkina Faso", "BF"}, {"Burundi", "BI"}, {"Cabo Verde", "CV"}, {"Cambodia", "KH"},
                {"Cameroon", "CM"}, {"Canada", "CA"}, {"Central African Republic", "CF"}, {"Chad", "TD"}, {"Chile", "CL"},
                {"China", "CN"}, {"Colombia", "CO"}, {"Comoros", "KM"}, {"Congo (Congo-Brazzaville)", "CG"}, {"Costa Rica", "CR"},
                {"Croatia", "HR"}, {"Cuba", "CU"}, {"Cyprus", "CY"}, {"Czechia (Czech Republic)", "CZ"}, {"Democratic Republic of the Congo", "CD"},
                {"Denmark", "DK"}, {"Djibouti", "DJ"}, {"Dominica", "DM"}, {"Dominican Republic", "DO"}, {"Ecuador", "EC"},
                {"Egypt", "EG"}, {"El Salvador", "SV"}, {"Equatorial Guinea", "GQ"}, {"Eritrea", "ER"}, {"Estonia", "EE"},
                {"Eswatini (fmr. 'Swaziland')", "SZ"}, {"Ethiopia", "ET"}, {"Fiji", "FJ"}, {"Finland", "FI"}, {"France", "FR"},
                {"Gabon", "GA"}, {"Gambia", "GM"}, {"Georgia", "GE"}, {"Germany", "DE"}, {"Ghana", "GH"}, {"Greece", "GR"},
                {"Grenada", "GD"}, {"Guatemala", "GT"}, {"Guinea", "GN"}, {"Guinea-Bissau", "GW"}, {"Guyana", "GY"},
                {"Haiti", "HT"}, {"Honduras", "HN"}, {"Hungary", "HU"}, {"Iceland", "IS"}, {"India", "IN"}, {"Indonesia", "ID"},
                {"Iran", "IR"}, {"Iraq", "IQ"}, {"Ireland", "IE"}, {"Israel", "IL"}, {"Italy", "IT"}, {"Jamaica", "JM"},
                {"Japan", "JP"}, {"Jordan", "JO"}, {"Kazakhstan", "KZ"}, {"Kenya", "KE"}, {"Kiribati", "KI"}, {"Kuwait", "KW"},
                {"Kyrgyzstan", "KG"}, {"Laos", "LA"}, {"Latvia", "LV"}, {"Lebanon", "LB"}, {"Lesotho", "LS"}, {"Liberia", "LR"},
                {"Libya", "LY"}, {"Liechtenstein", "LI"}, {"Lithuania", "LT"}, {"Luxembourg", "LU"}, {"Madagascar", "MG"},
                {"Malawi", "MW"}, {"Malaysia", "MY"}, {"Maldives", "MV"}, {"Mali", "ML"}, {"Malta", "MT"}, {"Marshall Islands", "MH"},
                {"Mauritania", "MR"}, {"Mauritius", "MU"}, {"Mexico", "MX"}, {"Micronesia", "FM"}, {"Moldova", "MD"}, {"Monaco", "MC"},
                {"Mongolia", "MN"}, {"Montenegro", "ME"}, {"Morocco", "MA"}, {"Mozambique", "MZ"}, {"Myanmar (formerly Burma)", "MM"},
                {"Namibia", "NA"}, {"Nauru", "NR"}, {"Nepal", "NP"}, {"Netherlands", "NL"}, {"New Zealand", "NZ"}, {"Nicaragua", "NI"},
                {"Niger", "NE"}, {"Nigeria", "NG"}, {"North Korea", "KP"}, {"North Macedonia", "MK"}, {"Norway", "NO"}, {"Oman", "OM"},
                {"Pakistan", "PK"}, {"Palau", "PW"}, {"Panama", "PA"}, {"Papua New Guinea", "PG"}, {"Paraguay", "PY"}, {"Peru", "PE"},
                {"Philippines", "PH"}, {"Poland", "PL"}, {"Portugal", "PT"}, {"Qatar", "QA"}, {"Romania", "RO"}, {"Russia", "RU"},
                {"Rwanda", "RW"}, {"Saint Kitts and Nevis", "KN"}, {"Saint Lucia", "LC"}, {"Saint Vincent and the Grenadines", "VC"},
                {"Samoa", "WS"}, {"San Marino", "SM"}, {"Sao Tome and Principe", "ST"}, {"Saudi Arabia", "SA"}, {"Senegal", "SN"},
                {"Serbia", "RS"}, {"Seychelles", "SC"}, {"Sierra Leone", "SL"}, {"Singapore", "SG"}, {"Slovakia", "SK"}, {"Slovenia", "SI"},
                {"Solomon Islands", "SB"}, {"Somalia", "SO"}, {"South Africa", "ZA"}, {"South Korea", "KR"}, {"South Sudan", "SS"},
                {"Spain", "ES"}, {"Sri Lanka", "LK"}, {"Sudan", "SD"}, {"Suriname", "SR"}, {"Sweden", "SE"}, {"Switzerland", "CH"},
                {"Syria", "SY"}, {"Taiwan", "TW"}, {"Tajikistan", "TJ"}, {"Tanzania", "TZ"}, {"Thailand", "TH"}, {"Timor-Leste", "TL"},
                {"Togo", "TG"}, {"Tonga", "TO"}, {"Trinidad and Tobago", "TT"}, {"Tunisia", "TN"}, {"Turkey", "TR"}, {"Turkmenistan", "TM"},
                {"Tuvalu", "TV"}, {"Uganda", "UG"}, {"Ukraine", "UA"}, {"United Arab Emirates", "AE"}, {"United Kingdom", "GB"},
                {"United States", "US"}, {"Uruguay", "UY"}, {"Uzbekistan", "UZ"}, {"Vanuatu", "VU"}, {"Vatican City", "VA"},
                {"Venezuela", "VE"}, {"Vietnam", "VN"}, {"Yemen", "YE"}, {"Zambia", "ZM"}, {"Zimbabwe", "ZW"}
            };

            // Get the country code for the selected country
            if (!countryCodeMap.TryGetValue(Player.Country, out string? countryCode))
            {
                ModelState.AddModelError("Player.PhoneNumber", "Invalid country selected.");
                return;
            }

            // Country-specific length validation
            var countryLengthRules = new Dictionary<string, int>
            {
                { "Israel", 10 },
                { "United States", 10 },
                { "United Kingdom", 10 },
                { "Germany", 10 },
                { "France", 9 },
                { "Canada", 10 },
                { "Australia", 9 },
                { "Japan", 10 },
                { "South Korea", 10 }
            };

            // Remove any non-digit characters for validation
            var digitsOnly = new string(Player.PhoneNumber.Where(char.IsDigit).ToArray());
            
            if (countryLengthRules.TryGetValue(Player.Country, out int expectedLength))
            {
                if (digitsOnly.Length != expectedLength)
                {
                    ModelState.AddModelError("Player.PhoneNumber", $"Phone number for {Player.Country} must be exactly {expectedLength} digits.");
                    return;
                }
            }
            else
            {
                // For other countries, require at least 10 digits
                if (digitsOnly.Length < 10)
                {
                    ModelState.AddModelError("Player.PhoneNumber", "Phone number must contain at least 10 digits.");
                    return;
                }
            }

            // Additional country-specific validation patterns
            var phoneValidationRules = new Dictionary<string, (string Pattern, string ErrorMessage)>
            {
                { "Israel", (@"^0\d{2}-\d{7}$", "Israeli phone number must be in format: 0XX-XXXXXXX (e.g., 052-1234567)") },
                { "United States", (@"^\(\d{3}\) \d{3}-\d{4}$", "US phone number must be in format: (XXX) XXX-XXXX (e.g., (555) 123-4567)") },
                { "United Kingdom", (@"^\+44 \d{4} \d{6}$", "UK phone number must be in format: +44 XXXX XXXXXX (e.g., +44 1234 567890)") },
                { "Germany", (@"^\+49 \d{3} \d{7,8}$", "German phone number must be in format: +49 XXX XXXXXXX (e.g., +49 30 12345678)") },
                { "France", (@"^\+33 \d \d{2} \d{2} \d{2} \d{2}$", "French phone number must be in format: +33 X XX XX XX XX (e.g., +33 1 23 45 67 89)") },
                { "Canada", (@"^\(\d{3}\) \d{3}-\d{4}$", "Canadian phone number must be in format: (XXX) XXX-XXXX (e.g., (416) 123-4567)") },
                { "Australia", (@"^\+61 \d \d{4} \d{4}$", "Australian phone number must be in format: +61 X XXXX XXXX (e.g., +61 2 1234 5678)") },
                { "Japan", (@"^\+81 \d{2} \d{4} \d{4}$", "Japanese phone number must be in format: +81 XX XXXX XXXX (e.g., +81 90 1234 5678)") },
                { "South Korea", (@"^\+82 \d{2} \d{4} \d{4}$", "Korean phone number must be in format: +82 XX XXXX XXXX (e.g., +82 10 1234 5678)") }
            };

            // Apply specific validation for known countries, otherwise use basic validation
            if (phoneValidationRules.TryGetValue(Player.Country, out var rule))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(Player.PhoneNumber, rule.Pattern))
                {
                    ModelState.AddModelError("Player.PhoneNumber", rule.ErrorMessage);
                }
            }
            else
            {
                // For other countries, use basic international format validation
                if (!System.Text.RegularExpressions.Regex.IsMatch(Player.PhoneNumber, @"^\+?\d{1,3}[\s\-]?\d{3,14}$"))
                {
                    ModelState.AddModelError("Player.PhoneNumber", "Phone number must be in international format (e.g., +1 1234567890 or +44 1234567890)");
                }
            }
        }
    }
}
