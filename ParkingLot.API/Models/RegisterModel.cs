using System.ComponentModel.DataAnnotations;

namespace ParkingLot.API.Models
{
    public class RegisterModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string? Role { get; set; }
    }
}
