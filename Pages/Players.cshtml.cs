using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Connect4Server.Data;
using Connect4Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Connect4Server.Pages
{
    public class PlayersModel : PageModel
    {
        private readonly Connect4Context _context;

        public PlayersModel(Connect4Context context)
        {
            _context = context;
        }

        public IList<Player> Players { get; set; } = default!;
        public bool QueryExecuted { get; set; } = false;
        public string QueryType { get; set; } = string.Empty;

        [BindProperty]
        public string Country { get; set; } = string.Empty;

        public void OnGet()
        {
            QueryExecuted = false;
        }

        public async Task OnPostShowAllAsync()
        {
            QueryExecuted = true;
            QueryType = "All Players";
            Players = await _context.Players
                .OrderBy(p => p.PlayerId)
                .ToListAsync();
        }

        public async Task OnPostShowWinnersAsync()
        {
            QueryExecuted = true;
            QueryType = "Players with Wins";
            Players = await _context.Players
                .Where(p => p.GamesWon > 0)
                .OrderByDescending(p => p.GamesWon)
                .ThenBy(p => p.PlayerId)
                .ToListAsync();
        }

        public async Task OnPostByCountryAsync()
        {
            QueryExecuted = true;

            if (string.IsNullOrWhiteSpace(Country))
            {
                QueryType = "All Players (No Country Specified)";
                Players = await _context.Players
                    .OrderBy(p => p.PlayerId)
                    .ToListAsync();
            }
            else
            {
                QueryType = $"Players from {Country}";
                Players = await _context.Players
                    .Where(p => p.Country.ToLower().Contains(Country.ToLower()))
                    .OrderBy(p => p.PlayerId)
                    .ToListAsync();
            }
        }

        public async Task OnPostTopPlayersAsync()
        {
            QueryExecuted = true;
            QueryType = "Top 10 Players by Win Rate";
            Players = await _context.Players
                .Where(p => p.GamesPlayed > 0)
                .OrderByDescending(p => (double)p.GamesWon / p.GamesPlayed)
                .ThenByDescending(p => p.GamesWon)
                .Take(10)
                .ToListAsync();
        }

        public async Task OnPostRecentPlayersAsync()
        {
            QueryExecuted = true;
            QueryType = "Recently Registered Players";
            Players = await _context.Players
                .OrderByDescending(p => p.RegistrationDate)
                .Take(15)
                .ToListAsync();
        }
    }
}
