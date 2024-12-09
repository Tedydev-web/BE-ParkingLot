namespace ParkingLot.Core.Entities;

public class PriceHistory
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ParkingLotId { get; set; } = string.Empty;
    public ParkingLot ParkingLot { get; set; } = null!;
    public Dictionary<string, decimal> PricesByVehicleType { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
