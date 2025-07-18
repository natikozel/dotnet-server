using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Connect4Server.Data;
using Connect4Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Connect4Server.Pages
{
    public class StatisticsModel : PageModel
    {
        private readonly Connect4Context _context;

        public StatisticsModel(Connect4Context context)
        {
            _context = context;
        }

        public IList<Player> AllPlayers { get; set; } = default!;
        public IList<PlayerWithLastGameDate> PlayersWithLastGame { get; set; } = default!;
        public IList<Game> RecentGames { get; set; } = default!;
        public IList<PlayerGameCount> PlayerGameCounts { get; set; } = default!;
        public IList<GameCountGroup> GameCountGroups { get; set; } = default!;
        public IList<CountryGroup> CountryGroups { get; set; } = default!;
        public Player? SelectedPlayer { get; set; }
        public IList<Game> SelectedPlayerGames { get; set; } = default!;

        [BindProperty]
        public string SortDirection { get; set; } = "asc";

        [BindProperty]
        public int SelectedPlayerId { get; set; }

        [BindProperty]
        public string ViewMode { get; set; } = "overview";

        public async Task OnGetAsync()
        {
            await LoadAllDataAsync();
        }

        public async Task OnPostAsync()
        {
            await LoadAllDataAsync();
            
            if (SelectedPlayerId > 0)
            {
                SelectedPlayer = await _context.Players.FindAsync(SelectedPlayerId);
                if (SelectedPlayer != null)
                {
                    SelectedPlayerGames = await _context.Games
                        .Where(g => g.PlayerId == SelectedPlayerId)
                        .OrderByDescending(g => g.StartTime)
                        .ToListAsync();
                }
            }
        }

        private async Task LoadAllDataAsync()
        {
            // Load all players with sorting
            var playersQuery = _context.Players.AsQueryable();
            if (SortDirection == "desc")
            {
                AllPlayers = await playersQuery
                    .OrderByDescending(p => p.FirstName.ToLower())
                    .ToListAsync();
            }
            else
            {
                AllPlayers = await playersQuery
                    .OrderBy(p => p.FirstName.ToLower())
                    .ToListAsync();
            }

            // Load all games once with player info
            var allGames = await _context.Games
                .Include(g => g.Player)
                .OrderByDescending(g => g.StartTime)
                .ToListAsync();

            // Load players with last game date
            PlayersWithLastGame = AllPlayers
                .Select(player => new PlayerWithLastGameDate
                {
                    FirstName = player.FirstName,
                    LastGameDate = allGames.Where(g => g.PlayerId == player.Id)
                                          .FirstOrDefault()?.StartTime
                })
                .OrderByDescending(p => p.FirstName)
                .ToList();

            // Load recent games (first game per player for distinct functionality)
            RecentGames = allGames
                .GroupBy(g => g.PlayerId)
                .Select(g => g.First())
                .OrderByDescending(g => g.StartTime)
                .ToList();

            // Load player game counts
            PlayerGameCounts = await _context.Players
                .Select(p => new PlayerGameCount
                {
                    FirstName = p.FirstName,
                    GamesPlayed = p.GamesPlayed
                })
                .OrderBy(p => p.FirstName)
                .ToListAsync();

            // Load players grouped by games played
            GameCountGroups = await _context.Players
                .GroupBy(p => p.GamesPlayed)
                .Select(g => new GameCountGroup
                {
                    GamesPlayedCount = g.Key,
                    Players = g.OrderBy(p => p.FirstName).ToList()
                })
                .OrderByDescending(g => g.GamesPlayedCount)
                .ToListAsync();

            // Load players grouped by country
            CountryGroups = await _context.Players
                .GroupBy(p => p.Country)
                .Select(g => new CountryGroup
                {
                    CountryName = g.Key,
                    Players = g.OrderBy(p => p.FirstName).ToList()
                })
                .OrderBy(g => g.CountryName)
                .ToListAsync();
        }
    }

    public class PlayerWithLastGameDate
    {
        public string FirstName { get; set; } = string.Empty;
        public DateTime? LastGameDate { get; set; }
    }

    public class PlayerGameCount
    {
        public string FirstName { get; set; } = string.Empty;
        public int GamesPlayed { get; set; }
    }

    public class GameCountGroup
    {
        public int GamesPlayedCount { get; set; }
        public List<Player> Players { get; set; } = new List<Player>();
    }

    public class CountryGroup
    {
        public string CountryName { get; set; } = string.Empty;
        public List<Player> Players { get; set; } = new List<Player>();
    }
} 