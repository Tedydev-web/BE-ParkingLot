namespace ParkingLot.Core.Entities;

public class Review
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int Rating { get; set; }
    public string Comment { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public string ParkingLotId { get; set; }
    public ParkingLot ParkingLot { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
} 