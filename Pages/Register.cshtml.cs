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

        private void ValidatePhoneNumber()
        {
            if (string.IsNullOrEmpty(Player.Country) || string.IsNullOrEmpty(Player.PhoneNumber))
                return;

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
                { "South Korea", (@"^\+82 \d{2} \d{4} \d{4}$", "Korean phone number must be in format: +82 XX XXXX XXXX (e.g., +82 10 1234 5678)") },
                { "Other", (@"^\+\d{1,3} \d{3,14}$", "International phone number must be in format: +XXX XXXXXXXXXX (e.g., +1 1234567890)") }
            };

            if (phoneValidationRules.TryGetValue(Player.Country, out var rule))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(Player.PhoneNumber, rule.Pattern))
                {
                    ModelState.AddModelError("Player.PhoneNumber", rule.ErrorMessage);
                }
            }
        }
    }
}
