using ParkingLot.Core.DTOs;

namespace ParkingLot.Application.DTOs;

public class CreateParkingLotDto : ICreateParkingLotDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Location
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    // Contact
    public string ContactPhone { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;

    // Business
    public decimal BaseHourlyRate { get; set; }
    public bool Is24Hours { get; set; }
    public TimeSpan? OpenTime { get; set; }
    public TimeSpan? CloseTime { get; set; }

    // Capacity
    public Dictionary<string, int> SlotTypes { get; set; } = new();
    public Dictionary<string, decimal> PricesByVehicleType { get; set; } = new();

    // Features  
    public bool HasCamera { get; set; }
    public bool HasRoof { get; set; }
    public bool HasOvernightParking { get; set; }
    public bool HasDisabledAccess { get; set; }
    public bool HasWashing { get; set; }
    public bool HasMaintenance { get; set; }

    public ICollection<string> Images { get; set; } = new List<string>();
}