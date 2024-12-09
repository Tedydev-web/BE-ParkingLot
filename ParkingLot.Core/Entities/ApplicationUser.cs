using Microsoft.AspNetCore.Identity;

namespace ParkingLot.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<ParkingLot> OwnedParkingLots { get; set; } = new List<ParkingLot>();
} 