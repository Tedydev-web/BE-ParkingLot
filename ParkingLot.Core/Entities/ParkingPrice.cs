namespace ParkingLot.Core.Entities;

public class ParkingPrice
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ParkingLotId { get; set; }
    public ParkingLot ParkingLot { get; set; }
    public SlotType VehicleType { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal DailyRate { get; set; }
    public decimal MonthlyRate { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsCurrentPrice => !EffectiveTo.HasValue;
} 