namespace ParkingLot.Core.Entities;

public class ParkingSlot
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SlotNumber { get; set; }
    public bool IsOccupied { get; set; }
    public SlotType Type { get; set; }
    public string ParkingLotId { get; set; }
    public ParkingLot ParkingLot { get; set; }
}

public enum SlotType
{
    Car,
    Motorbike,
    Bicycle,
    Electric
} 