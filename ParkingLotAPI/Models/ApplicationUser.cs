using Microsoft.AspNetCore.Identity;
using ParkingLotAPI.Models.Auth;

namespace ParkingLotAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
    }
} 