using Microsoft.EntityFrameworkCore;
using Connect4Server.Data;
using Connect4Server.Models;

namespace Connect4Server.Services
{
    public class GameTimeoutService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<GameTimeoutService> _logger;
        private const int GAME_TIMEOUT_MINUTES = 5;
        private const int CLEANUP_INTERVAL_MINUTES = 2; // Run cleanup every 2 minutes

        public GameTimeoutService(IServiceProvider serviceProvider, ILogger<GameTimeoutService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupTimedOutGames();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during game timeout cleanup");
                }

                // Wait for the next cleanup cycle
                await Task.Delay(TimeSpan.FromMinutes(CLEANUP_INTERVAL_MINUTES), stoppingToken);
            }
        }

        private async Task CleanupTimedOutGames()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<Connect4Context>();

            var timeoutThreshold = DateTime.Now.AddMinutes(-GAME_TIMEOUT_MINUTES);
            var timedOutGames = await context.Games
                .Include(g => g.Player)
                .Where(g => g.Status == "InProgress" && g.StartTime < timeoutThreshold)
                .ToListAsync();

            if (timedOutGames.Any())
            {
                int cleanedCount = 0;
                foreach (var game in timedOutGames)
                {
                    game.Status = "Lost";
                    game.Winner = "CPU";
                    game.EndTime = DateTime.Now;
                    game.Player.GamesPlayed++;
                    game.Player.GamesLost++;
                    cleanedCount++;
                }

                await context.SaveChangesAsync();
                _logger.LogInformation($"Cleaned up {cleanedCount} timed-out games");
            }
        }
    }
} 