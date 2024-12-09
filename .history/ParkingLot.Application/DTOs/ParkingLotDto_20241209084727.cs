using ParkingLot.Core.DTOs;
using ParkingLot.Core.Entities;

namespace ParkingLot.Application.DTOs;

public class ParkingLotDto : IParkingLotDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string ContactPhone { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty; 
    public string Website { get; set; } = string.Empty;
    public decimal BaseHourlyRate { get; set; }
    public bool Is24Hours { get; set; }
    public string OpenTime { get; set; } = string.Empty;
    public string CloseTime { get; set; } = string.Empty;
    public Dictionary<string, int> SlotTypes { get; set; } = new();
    public Dictionary<string, decimal> PricesByVehicleType { get; set; } = new();
    public bool HasCamera { get; set; }
    public bool HasRoof { get; set; }
    public bool HasOvernightParking { get; set; }
    public bool HasDisabledAccess { get; set; }
    public bool HasWashing { get; set; }
    public bool HasMaintenance { get; set; }
    public List<string> Images { get; set; } = new List<string>();
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string LastModifiedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
}