using ParkingLot.Core.Entities;

namespace ParkingLot.Application.DTOs;

public class ParkingSlotDto
{
    public string Id { get; set; } = string.Empty;
    public string ParkingLotId { get; set; } = string.Empty;
    public string SlotNumber { get; set; } = string.Empty;
    public SlotType Type { get; set; }
    public bool IsOccupied { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
} 