using System.ComponentModel.DataAnnotations;

namespace Connect4Server.Models
{
    public class Player
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [MinLength(2, ErrorMessage = "First name must be at least 2 characters long")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Player ID is required")]
        [Range(1, 1000, ErrorMessage = "Player ID must be between 1 and 1000")]
        [Display(Name = "Player ID")]
        public int PlayerId { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^0\d{2}-\d{7}$", ErrorMessage = "Phone number must be in Israeli format (e.g., 052-1234567)")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required")]
        [Display(Name = "Country")]
        public string Country { get; set; } = string.Empty;

        [Display(Name = "Registration Date")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [Display(Name = "Games Played")]
        public int GamesPlayed { get; set; } = 0;

        [Display(Name = "Games Won")]
        public int GamesWon { get; set; } = 0;

        [Display(Name = "Games Lost")]
        public int GamesLost { get; set; } = 0;
    }
} 