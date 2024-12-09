using ParkingLot.Core.DTOs;

namespace ParkingLot.Application.DTOs;

public class UpdateParkingLotDto : IUpdateParkingLotDto 
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Contact
    public string ContactPhone { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Business
    public decimal BaseHourlyRate { get; set; }
    public bool Is24Hours { get; set; }
    public TimeSpan? OpenTime { get; set; }
    public TimeSpan? CloseTime { get; set; }
    public Dictionary<string, decimal> PricesByVehicleType { get; set; } = new();

    // Features
    public bool IsActive { get; set; }
    public bool HasCamera { get; set; } 
    public bool HasRoof { get; set; }
    public bool HasOvernightParking { get; set; }
    public bool HasDisabledAccess { get; set; }
    public bool HasWashing { get; set; }
    public bool HasMaintenance { get; set; }

    public ICollection<string> Images { get; set; } = new List<string>();

    public string Address { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}