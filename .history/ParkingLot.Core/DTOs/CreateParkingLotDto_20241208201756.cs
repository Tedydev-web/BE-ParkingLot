using ParkingLot.Core.Entities;

namespace ParkingLot.Core.DTOs;

public class CreateParkingLotDto : ICreateParkingLotDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string LocationId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Dictionary<string, int> SlotTypes { get; set; } = new();
    public Dictionary<string, decimal> PricesByVehicleType { get; set; } = new();
    public bool HasCamera { get; set; }
    public bool HasRoof { get; set; }
    public bool HasOvernightParking { get; set; }
    public bool HasDisabledAccess { get; set; }
}
