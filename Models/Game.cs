using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Connect4Server.Models
{
    public class Game
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Player")]
        public int PlayerId { get; set; }

        [Display(Name = "Game Start Time")]
        public DateTime StartTime { get; set; } = DateTime.Now;

        [Display(Name = "Game End Time")]
        public DateTime? EndTime { get; set; }

        [Display(Name = "Game Status")]
        public string Status { get; set; } = "InProgress"; // InProgress, Won, Lost, Draw

        [Display(Name = "Board State")]
        public string BoardState { get; set; } = string.Empty; // JSON representation of the board

        [Display(Name = "Current Player")]
        public string CurrentPlayer { get; set; } = "Player"; // Player or CPU

        [Display(Name = "Winner")]
        public string? Winner { get; set; }

        // Navigation property
        public virtual Player Player { get; set; } = null!;
    }
} 