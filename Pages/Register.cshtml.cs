using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Connect4Server.Models;
using Connect4Server.Data;
using Microsoft.EntityFrameworkCore;

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
        public Player Player { get; set; } = default!;

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            Player = new Player();
        }

        /// <summary>
        /// Handles player registration form submission with comprehensive validation.
        /// Validates form data, checks for duplicate player IDs, and saves new player to database.
        /// Provides detailed error messages for validation failures and success confirmation.
        /// </summary>
        /// <returns>PageResult with success or error message</returns>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .Select(x => $"{x.Key}: {string.Join(", ", x.Value?.Errors.Select(e => e.ErrorMessage) ?? new string[0])}")
                    .ToList();
                
                ErrorMessage = $"Please fix the following errors: {string.Join("; ", errors)}";
                return Page();
            }

            if (Player == null)
            {
                ErrorMessage = "Player data is missing. Please try again.";
                return Page();
            }

            var existingPlayer = await _context.Players
                .FirstOrDefaultAsync(p => p.PlayerId == Player.PlayerId);

            if (existingPlayer != null)
            {
                ErrorMessage = $"Player ID {Player.PlayerId} is already taken. Please choose a different ID.";
                return Page();
            }

            try
            {
                await _context.Database.EnsureCreatedAsync();
                
                Player.RegistrationDate = DateTime.Now;
                _context.Players.Add(Player);
                await _context.SaveChangesAsync();

                SuccessMessage = $"🎉 Welcome {Player.FirstName}! You have been successfully registered with Player ID {Player.PlayerId}. You can now start playing Connect 4!";
                
                Player = new Player();
                
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred during registration: {ex.Message}. Please try again.";
                Console.WriteLine($"Registration error: {ex}");
                return Page();
            }
        }
    }
} 