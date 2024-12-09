using ParkingLot.Core.Entities;

namespace ParkingLot.Application.DTOs;

public class ParkingPriceDto
{
    public string Id { get; set; } = string.Empty;
    public string ParkingLotId { get; set; } = string.Empty;
    public SlotType VehicleType { get; set; }
    public decimal HourlyRate { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
} 