using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Connect4Server.Data;
using Connect4Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Connect4Server.Pages
{
    public class ManagementModel : PageModel
    {
        private readonly Connect4Context _context;

        public ManagementModel(Connect4Context context)
        {
            _context = context;
        }

        public IList<Player> Players { get; set; } = default!;
        public IList<Game> Games { get; set; } = default!;
        public bool QueryExecuted { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public bool IsSuccess { get; set; } = false;

        [BindProperty]
        public Player PlayerToUpdate { get; set; } = default!;

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnPostDeletePlayerAsync(int playerId)
        {
            try
            {
                var player = await _context.Players.FindAsync(playerId);
                if (player == null)
                {
                    Message = "Player not found.";
                    IsSuccess = false;
                    await LoadDataAsync();
                    return Page();
                }

                // Delete all games associated with this player first
                var playerGames = await _context.Games.Where(g => g.PlayerId == playerId).ToListAsync();
                _context.Games.RemoveRange(playerGames);

                // Delete the player
                _context.Players.Remove(player);
                await _context.SaveChangesAsync();

                Message = $"Player {player.FirstName} and all their games have been deleted successfully.";
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                Message = $"Error deleting player: {ex.Message}";
                IsSuccess = false;
            }

            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteGameAsync(int gameId)
        {
            try
            {
                var game = await _context.Games.Include(g => g.Player).FirstOrDefaultAsync(g => g.Id == gameId);
                if (game == null)
                {
                    Message = "Game not found.";
                    IsSuccess = false;
                    await LoadDataAsync();
                    return Page();
                }

                // Update player statistics
                var player = game.Player;
                player.GamesPlayed = Math.Max(0, player.GamesPlayed - 1);
                
                if (game.Status == "Won")
                {
                    player.GamesWon = Math.Max(0, player.GamesWon - 1);
                }
                else if (game.Status == "Lost")
                {
                    player.GamesLost = Math.Max(0, player.GamesLost - 1);
                }

                // Delete the game
                _context.Games.Remove(game);
                await _context.SaveChangesAsync();

                Message = $"Game (ID: {gameId}) has been deleted successfully.";
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                Message = $"Error deleting game: {ex.Message}";
                IsSuccess = false;
            }

            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostUpdatePlayerAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    Message = "Please correct the validation errors.";
                    IsSuccess = false;
                    await LoadDataAsync();
                    return Page();
                }

                var existingPlayer = await _context.Players.FindAsync(PlayerToUpdate.Id);
                if (existingPlayer == null)
                {
                    Message = "Player not found.";
                    IsSuccess = false;
                    await LoadDataAsync();
                    return Page();
                }

                // Check if PlayerId is unique (excluding current player)
                var duplicatePlayer = await _context.Players
                    .FirstOrDefaultAsync(p => p.PlayerId == PlayerToUpdate.PlayerId && p.Id != PlayerToUpdate.Id);
                if (duplicatePlayer != null)
                {
                    Message = "Player ID already exists. Please choose a different Player ID.";
                    IsSuccess = false;
                    await LoadDataAsync();
                    return Page();
                }

                // Update player details
                existingPlayer.FirstName = PlayerToUpdate.FirstName;
                existingPlayer.PlayerId = PlayerToUpdate.PlayerId;
                existingPlayer.PhoneNumber = PlayerToUpdate.PhoneNumber;
                existingPlayer.Country = PlayerToUpdate.Country;

                await _context.SaveChangesAsync();

                Message = $"Player {existingPlayer.FirstName} has been updated successfully.";
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                Message = $"Error updating player: {ex.Message}";
                IsSuccess = false;
            }

            await LoadDataAsync();
            return Page();
        }

        private async Task LoadDataAsync()
        {
            QueryExecuted = true;
            Players = await _context.Players.OrderBy(p => p.FirstName).ToListAsync();
            Games = await _context.Games.Include(g => g.Player).OrderByDescending(g => g.StartTime).ToListAsync();
        }
    }
} 