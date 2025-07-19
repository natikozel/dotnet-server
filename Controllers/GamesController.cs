using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Connect4Server.Data;
using Connect4Server.Models;
using System.Text.Json;

namespace Connect4Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly Connect4Context _context;
        private readonly Random _random;
        private const int GAME_TIMEOUT_MINUTES = 5;

        public GamesController(Connect4Context context)
        {
            _context = context;
            _random = new Random();
        }

        /// <summary>
        /// Checks if a game has timed out (been InProgress for more than 5 minutes)
        /// and automatically marks it as "Lost" if so.
        /// </summary>
        /// <param name="game">The game to check for timeout</param>
        /// <returns>True if the game was timed out, false otherwise</returns>
        private async Task<bool> CheckAndHandleGameTimeout(Game game)
        {
            if (game.Status == "InProgress" && game.StartTime.AddMinutes(GAME_TIMEOUT_MINUTES) < DateTime.Now)
            {
                // Game has timed out - mark as lost
                game.Status = "Lost";
                game.Winner = "CPU";
                game.EndTime = DateTime.Now;
                game.Player.GamesPlayed++;
                game.Player.GamesLost++;
                
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Starts a new Connect 4 game for a registered player.
        /// Creates a new game record with empty board state and initializes game settings.
        /// </summary>
        /// <param name="request">Contains the player ID to start the game</param>
        /// <returns>Response with new game data or error message</returns>
        [HttpPost("start")]
        public async Task<ActionResult<StartGameResponse>> StartGame([FromBody] StartGameRequest request)
        {
            try
            {
                var player = await _context.Players.FirstOrDefaultAsync(p => p.PlayerId == request.PlayerId);
                if (player == null)
                {
                    return Ok(new StartGameResponse
                    {
                        Success = false,
                        Message = "Player not found. Please register first."
                    });
                }

                // Create empty board as jagged array for proper JSON serialization
                var emptyBoard = new int[6][];
                for (int i = 0; i < 6; i++)
                {
                    emptyBoard[i] = new int[7];
                }

                var game = new Game
                {
                    PlayerId = player.Id,
                    StartTime = DateTime.Now,
                    Status = "InProgress",
                    BoardState = JsonSerializer.Serialize(emptyBoard),
                    CurrentPlayer = "Player",
                    Winner = null
                };

                _context.Games.Add(game);
                await _context.SaveChangesAsync();

                game = await _context.Games.Include(g => g.Player)
                    .FirstOrDefaultAsync(g => g.Id == game.Id);

                return Ok(new StartGameResponse
                {
                    Success = true,
                    Message = "Game started successfully",
                    Game = MapToDto(game!)
                });
            }
            catch (Exception ex)
            {
                return Ok(new StartGameResponse
                {
                    Success = false,
                    Message = $"Error starting game: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Processes a player's move in Connect 4 game, validates the move, updates game state,
        /// and triggers CPU response. Handles complete game flow including win detection and turn management.
        /// </summary>
        /// <param name="request">Contains game ID and column number for the move</param>
        /// <returns>Response with updated game state and CPU move if applicable</returns>
        [HttpPost("move")]
        public async Task<ActionResult<MakeMoveResponse>> MakeMove([FromBody] MakeMoveRequest request)
        {
            try
            {
                var game = await _context.Games.Include(g => g.Player)
                    .FirstOrDefaultAsync(g => g.Id == request.GameId);

                if (game == null)
                {
                    return Ok(new MakeMoveResponse
                    {
                        Success = false,
                        Message = "Game not found"
                    });
                }

                // Check for game timeout before processing the move
                var wasTimedOut = await CheckAndHandleGameTimeout(game);
                if (wasTimedOut)
                {
                    return Ok(new MakeMoveResponse
                    {
                        Success = false,
                        Message = "Game has timed out and was marked as lost"
                    });
                }

                if (game.Status != "InProgress")
                {
                    return Ok(new MakeMoveResponse
                    {
                        Success = false,
                        Message = "Game is not in progress"
                    });
                }

                if (game.CurrentPlayer != "Player")
                {
                    return Ok(new MakeMoveResponse
                    {
                        Success = false,
                        Message = "It's not your turn"
                    });
                }

                // Deserialize jagged array and convert to 2D array for game logic
                var boardJagged = JsonSerializer.Deserialize<int[][]>(game.BoardState);
                if (boardJagged == null)
                {
                    boardJagged = new int[6][];
                    for (int i = 0; i < 6; i++)
                    {
                        boardJagged[i] = new int[7];
                    }
                }
                
                // Convert to 2D array for existing game logic
                var board = new int[6, 7];
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 7; j++)
                    {
                        board[i, j] = boardJagged[i][j];
                    }
                }

                if (request.Column < 0 || request.Column > 6)
                {
                    return Ok(new MakeMoveResponse
                    {
                        Success = false,
                        Message = "Invalid column"
                    });
                }

                if (board[0, request.Column] != 0)
                {
                    return Ok(new MakeMoveResponse
                    {
                        Success = false,
                        Message = "Column is full"
                    });
                }

                // Drop the player's piece in the selected column (gravity effect)
                for (int row = 5; row >= 0; row--)
                {
                    if (board[row, request.Column] == 0)
                    {
                        board[row, request.Column] = 1;
                        break;
                    }
                }

                // Check if player won with this move
                if (CheckWin(board, 1))
                {
                    game.Status = "Won";
                    game.Winner = "Player";
                    game.EndTime = DateTime.Now;
                    game.Player.GamesPlayed++;
                    game.Player.GamesWon++;
                }
                else if (IsBoardFull(board))
                {
                    game.Status = "Draw";
                    game.Winner = "Draw";
                    game.EndTime = DateTime.Now;
                    game.Player.GamesPlayed++;
                }
                
                // Store CPU move BEFORE placing it on board to return the correct move
                int? actualCpuMove = null;
                if (game.Status == "InProgress")
                {
                    // CPU makes its move if game continues
                    var cpuMove = MakeCpuMove(board);
                    actualCpuMove = cpuMove;
                    
                    if (cpuMove != -1)
                    {
                        // Drop CPU's piece in selected column
                        for (int row = 5; row >= 0; row--)
                        {
                            if (board[row, cpuMove] == 0)
                            {
                                board[row, cpuMove] = 2;
                                break;
                            }
                        }

                        // Check if CPU won
                        if (CheckWin(board, 2))
                        {
                            game.Status = "Lost";
                            game.Winner = "CPU";
                            game.EndTime = DateTime.Now;
                            game.Player.GamesPlayed++;
                            game.Player.GamesLost++;
                        }
                        else if (IsBoardFull(board))
                        {
                            game.Status = "Draw";
                            game.Winner = "Draw";
                            game.EndTime = DateTime.Now;
                            game.Player.GamesPlayed++;
                        }
                    }
                }

                // Convert 2D array back to jagged array for serialization
                var updatedBoardJagged = new int[6][];
                for (int i = 0; i < 6; i++)
                {
                    updatedBoardJagged[i] = new int[7];
                    for (int j = 0; j < 7; j++)
                    {
                        updatedBoardJagged[i][j] = board[i, j];
                    }
                }
                game.BoardState = JsonSerializer.Serialize(updatedBoardJagged);
                game.CurrentPlayer = game.Status == "InProgress" ? "Player" : game.CurrentPlayer;

                await _context.SaveChangesAsync();

                return Ok(new MakeMoveResponse
                {
                    Success = true,
                    Message = "Move made successfully",
                    Game = MapToDto(game),
                    CpuMove = actualCpuMove // Return the actual CPU move that was made
                });
            }
            catch (Exception ex)
            {
                return Ok(new MakeMoveResponse
                {
                    Success = false,
                    Message = $"Error making move: {ex.Message}"
                });
            }
        }

        [HttpGet("{gameId}")]
        public async Task<ActionResult<GameDto>> GetGame(int gameId)
        {
            var game = await _context.Games.Include(g => g.Player)
                .FirstOrDefaultAsync(g => g.Id == gameId);

            if (game == null)
            {
                return NotFound();
            }

            // Check for game timeout
            await CheckAndHandleGameTimeout(game);

            return Ok(MapToDto(game));
        }

        /// <summary>
        /// Cleans up all timed-out games by marking them as "Lost".
        /// This endpoint can be called periodically to clean up abandoned games.
        /// </summary>
        /// <returns>Response with cleanup results</returns>
        [HttpPost("cleanup-timeouts")]
        public async Task<ActionResult<CleanupTimeoutsResponse>> CleanupTimeouts()
        {
            try
            {
                var timeoutThreshold = DateTime.Now.AddMinutes(-GAME_TIMEOUT_MINUTES);
                var timedOutGames = await _context.Games
                    .Include(g => g.Player)
                    .Where(g => g.Status == "InProgress" && g.StartTime < timeoutThreshold)
                    .ToListAsync();

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

                if (cleanedCount > 0)
                {
                    await _context.SaveChangesAsync();
                }

                return Ok(new CleanupTimeoutsResponse
                {
                    Success = true,
                    Message = $"Cleaned up {cleanedCount} timed-out games",
                    CleanedCount = cleanedCount
                });
            }
            catch (Exception ex)
            {
                return Ok(new CleanupTimeoutsResponse
                {
                    Success = false,
                    Message = $"Error cleaning up timeouts: {ex.Message}",
                    CleanedCount = 0
                });
            }
        }

        /// <summary>
        /// Converts a Game entity to a GameDto for API response.
        /// Deserializes board state from JSON and includes player statistics.
        /// </summary>
        /// <param name="game">The game entity from database</param>
        /// <returns>GameDto with deserialized board and player data</returns>
        private GameDto MapToDto(Game game)
        {
            // Deserialize jagged array directly (no conversion needed)
            var boardJagged = JsonSerializer.Deserialize<int[][]>(game.BoardState);
            
            // Create empty board if deserialization fails
            if (boardJagged == null)
            {
                boardJagged = new int[6][];
                for (int i = 0; i < 6; i++)
                {
                    boardJagged[i] = new int[7];
                }
            }
            
            return new GameDto
            {
                Id = game.Id,
                PlayerId = game.PlayerId,
                StartTime = game.StartTime,
                EndTime = game.EndTime,
                Status = game.Status,
                Board = boardJagged, // Use jagged array directly
                CurrentPlayer = game.CurrentPlayer,
                Winner = game.Winner,
                Player = game.Player != null ? new PlayerDto
                {
                    Id = game.Player.Id,
                    FirstName = game.Player.FirstName,
                    PlayerId = game.Player.PlayerId,
                    PhoneNumber = game.Player.PhoneNumber,
                    Country = game.Player.Country,
                    GamesPlayed = game.Player.GamesPlayed,
                    GamesWon = game.Player.GamesWon,
                    GamesLost = game.Player.GamesLost
                } : null
            };
        }

        /// <summary>
        /// Generates a random CPU move for Connect 4 game.
        /// Identifies all valid columns (not full) and selects one randomly.
        /// </summary>
        /// <param name="board">Current game board state (6x7 array)</param>
        /// <returns>Column index (0-6) for CPU move, or -1 if no valid moves</returns>
        private int MakeCpuMove(int[,] board)
        {
            var validColumns = new List<int>();
            for (int col = 0; col < 7; col++)
            {
                if (board[0, col] == 0)
                {
                    validColumns.Add(col);
                }
            }

            if (validColumns.Count == 0)
                return -1;

            return validColumns[_random.Next(validColumns.Count)];
        }

        /// <summary>
        /// Checks if a player has won by getting 4 pieces in a row.
        /// Implements Connect 4 win detection algorithm checking all possible winning patterns:
        /// - Horizontal: 4 consecutive pieces in same row
        /// - Vertical: 4 consecutive pieces in same column  
        /// - Diagonal: 4 consecutive pieces in diagonal direction (both ways)
        /// </summary>
        /// <param name="board">Current game board state (6x7 array)</param>
        /// <param name="player">Player identifier (1 for human, 2 for CPU)</param>
        /// <returns>True if player has won, false otherwise</returns>
        private bool CheckWin(int[,] board, int player)
        {
            // Check horizontal wins (4 in a row)
            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    if (board[row, col] == player && board[row, col + 1] == player &&
                        board[row, col + 2] == player && board[row, col + 3] == player)
                        return true;
                }
            }

            // Check vertical wins (4 in a column)
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    if (board[row, col] == player && board[row + 1, col] == player &&
                        board[row + 2, col] == player && board[row + 3, col] == player)
                        return true;
                }
            }

            // Check diagonal wins (top-left to bottom-right)
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    if (board[row, col] == player && board[row + 1, col + 1] == player &&
                        board[row + 2, col + 2] == player && board[row + 3, col + 3] == player)
                        return true;
                }
            }

            // Check diagonal wins (top-right to bottom-left)
            for (int row = 0; row < 3; row++)
            {
                for (int col = 3; col < 7; col++)
                {
                    if (board[row, col] == player && board[row + 1, col - 1] == player &&
                        board[row + 2, col - 2] == player && board[row + 3, col - 3] == player)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the Connect 4 board is completely full (no more moves possible).
        /// Only needs to check the top row since pieces fall down due to gravity.
        /// </summary>
        /// <param name="board">Current game board state (6x7 array)</param>
        /// <returns>True if board is full, false if moves are still possible</returns>
        private bool IsBoardFull(int[,] board)
        {
            for (int col = 0; col < 7; col++)
            {
                if (board[0, col] == 0)
                    return false;
            }
            return true;
        }
    }
} 
