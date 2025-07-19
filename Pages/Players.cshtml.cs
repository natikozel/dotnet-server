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
        public List<TableColumn> TableColumns { get; set; } = new List<TableColumn>();

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
            
            TableColumns = new List<TableColumn>
            {
                new TableColumn { Header = "Player", Property = "Player", Type = "PlayerInfo" },
                new TableColumn { Header = "Contact", Property = "PhoneNumber", Type = "Text" },
                new TableColumn { Header = "Country", Property = "Country", Type = "Text" },
                new TableColumn { Header = "Registered", Property = "RegistrationDate", Type = "Date" },
                new TableColumn { Header = "Games", Property = "GamesPlayed", Type = "Badge" },
                new TableColumn { Header = "Won", Property = "GamesWon", Type = "Badge" },
                new TableColumn { Header = "Lost", Property = "GamesLost", Type = "Badge" },
                new TableColumn { Header = "Win Rate", Property = "WinRate", Type = "Percentage" }
            };
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
            
            TableColumns = new List<TableColumn>
            {
                new TableColumn { Header = "Player", Property = "Player", Type = "PlayerInfo" },
                new TableColumn { Header = "Contact", Property = "PhoneNumber", Type = "Text" },
                new TableColumn { Header = "Country", Property = "Country", Type = "Text" },
                new TableColumn { Header = "Registered", Property = "RegistrationDate", Type = "Date" },
                new TableColumn { Header = "Games", Property = "GamesPlayed", Type = "Badge" },
                new TableColumn { Header = "Won", Property = "GamesWon", Type = "Badge" },
                new TableColumn { Header = "Lost", Property = "GamesLost", Type = "Badge" },
                new TableColumn { Header = "Win Rate", Property = "WinRate", Type = "Percentage" }
            };
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
            
            TableColumns = new List<TableColumn>
            {
                new TableColumn { Header = "Player", Property = "Player", Type = "PlayerInfo" },
                new TableColumn { Header = "Contact", Property = "PhoneNumber", Type = "Text" },
                new TableColumn { Header = "Country", Property = "Country", Type = "Text" },
                new TableColumn { Header = "Registered", Property = "RegistrationDate", Type = "Date" },
                new TableColumn { Header = "Games", Property = "GamesPlayed", Type = "Badge" },
                new TableColumn { Header = "Won", Property = "GamesWon", Type = "Badge" },
                new TableColumn { Header = "Lost", Property = "GamesLost", Type = "Badge" },
                new TableColumn { Header = "Win Rate", Property = "WinRate", Type = "Percentage" }
            };
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
            
            TableColumns = new List<TableColumn>
            {
                new TableColumn { Header = "Player", Property = "Player", Type = "PlayerInfo" },
                new TableColumn { Header = "Contact", Property = "PhoneNumber", Type = "Text" },
                new TableColumn { Header = "Country", Property = "Country", Type = "Text" },
                new TableColumn { Header = "Registered", Property = "RegistrationDate", Type = "Date" },
                new TableColumn { Header = "Games", Property = "GamesPlayed", Type = "Badge" },
                new TableColumn { Header = "Won", Property = "GamesWon", Type = "Badge" },
                new TableColumn { Header = "Lost", Property = "GamesLost", Type = "Badge" },
                new TableColumn { Header = "Win Rate", Property = "WinRate", Type = "Percentage" }
            };
        }

        public async Task OnPostRecentPlayersAsync()
        {
            QueryExecuted = true;
            QueryType = "Recently Registered Players";
            Players = await _context.Players
                .OrderByDescending(p => p.RegistrationDate)
                .Take(15)
                .ToListAsync();
            
            TableColumns = new List<TableColumn>
            {
                new TableColumn { Header = "Player", Property = "Player", Type = "PlayerInfo" },
                new TableColumn { Header = "Contact", Property = "PhoneNumber", Type = "Text" },
                new TableColumn { Header = "Country", Property = "Country", Type = "Text" },
                new TableColumn { Header = "Registered", Property = "RegistrationDate", Type = "Date" },
                new TableColumn { Header = "Games", Property = "GamesPlayed", Type = "Badge" },
                new TableColumn { Header = "Won", Property = "GamesWon", Type = "Badge" },
                new TableColumn { Header = "Lost", Property = "GamesLost", Type = "Badge" },
                new TableColumn { Header = "Win Rate", Property = "WinRate", Type = "Percentage" }
            };
        }

        public async Task OnPostShowPlayersWithLastGameAsync()
        {
            QueryExecuted = true;
            QueryType = "All Players with Last Game (Sorted by Name Descending)";
            
            var query = from p in _context.Players
                       join g in _context.Games on p.Id equals g.PlayerId into gameGroup
                       from lastGame in gameGroup.OrderByDescending(g => g.EndTime ?? g.StartTime).Take(1).DefaultIfEmpty()
                       orderby p.FirstName descending
                       select new PlayerWithLastGame
                       {
                           Player = p,
                           LastGame = lastGame
                       };

            var result = await query.ToListAsync();
            
            Players = result.Select(r => r.Player).ToList();
            ViewData["LastGames"] = result.ToDictionary(r => r.Player.Id, r => r.LastGame);
            
            // Only 2 columns for this specific query
            TableColumns = new List<TableColumn>
            {
                new TableColumn { Header = "Player Name", Property = "Player", Type = "PlayerNameOnly" },
                new TableColumn { Header = "Last Game Date", Property = "LastGame", Type = "LastGameDate" }
            };
        }

        public async Task OnPostShowGamesWithoutDuplicatesAsync()
        {
            QueryExecuted = true;
            QueryType = "Games Without Duplicates (Unique Players)";
            
            try
            {
                // Get all players who have played games
                var playersWithGames = await _context.Players
                    .Where(p => p.GamesPlayed > 0)
                    .ToListAsync();

                // Get the first game for each player who has games
                var uniqueGames = new List<Game>();
                foreach (var player in playersWithGames)
                {
                    var firstGame = await _context.Games
                        .Where(g => g.PlayerId == player.Id)
                        .OrderBy(g => g.StartTime)
                        .FirstOrDefaultAsync();
                    
                    if (firstGame != null)
                    {
                        uniqueGames.Add(firstGame);
                    }
                }

                // Order by game start time
                uniqueGames = uniqueGames.OrderBy(g => g.StartTime).ToList();
                
                // Convert to players list for display
                Players = uniqueGames.Select(g => g.Player).ToList();
                ViewData["UniqueGames"] = uniqueGames.ToDictionary(g => g.Player.Id, g => g);
                
                // Table columns for games without duplicates
                TableColumns = new List<TableColumn>
                {
                    new TableColumn { Header = "Player", Property = "Player", Type = "PlayerInfo" },
                    new TableColumn { Header = "Game Start", Property = "GameStart", Type = "GameStartDate" },
                    new TableColumn { Header = "Game Status", Property = "GameStatus", Type = "GameStatus" },
                    new TableColumn { Header = "Winner", Property = "Winner", Type = "Winner" }
                };
            }
            catch (Exception)
            {
                // Fallback to showing all players if there's an error
                Players = await _context.Players.ToListAsync();
                TableColumns = new List<TableColumn>
                {
                    new TableColumn { Header = "Player", Property = "Player", Type = "PlayerInfo" },
                    new TableColumn { Header = "Contact", Property = "PhoneNumber", Type = "Text" },
                    new TableColumn { Header = "Country", Property = "Country", Type = "Text" },
                    new TableColumn { Header = "Games", Property = "GamesPlayed", Type = "Badge" }
                };
            }
        }
    }

    public class PlayerWithLastGame
    {
        public Player Player { get; set; } = null!;
        public Game? LastGame { get; set; }
    }

    public class TableColumn
    {
        public string Header { get; set; } = string.Empty;
        public string Property { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}
