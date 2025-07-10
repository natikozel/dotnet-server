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

        [BindProperty]
        public string Country { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public async Task OnPostShowAllAsync()
        {
            QueryExecuted = true;
            Players = await _context.Players
                .OrderBy(p => p.PlayerId)
                .ToListAsync();
        }

        public async Task OnPostShowWinnersAsync()
        {
            QueryExecuted = true;
            Players = await _context.Players
                .Where(p => p.GamesWon > 0)
                .OrderByDescending(p => p.GamesWon)
                .ToListAsync();
        }

        public async Task OnPostByCountryAsync()
        {
            QueryExecuted = true;
            if (!string.IsNullOrEmpty(Country))
            {
                Players = await _context.Players
                    .Where(p => p.Country.ToLower().Contains(Country.ToLower()))
                    .OrderBy(p => p.FirstName)
                    .ToListAsync();
            }
            else
            {
                Players = new List<Player>();
            }
        }
    }
} 