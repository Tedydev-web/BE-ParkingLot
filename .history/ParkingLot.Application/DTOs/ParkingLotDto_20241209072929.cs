using ParkingLot.Core.DTOs;
using ParkingLot.Core.Entities;

namespace ParkingLot.Application.DTOs;

public class ParkingLotDto : IParkingLotDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Location
    public string LocationId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    // Capacity 
    public int TotalSlots { get; set; }
    public int AvailableSlots { get; set; }
    public Dictionary<string, int> SlotTypeCapacity { get; set; } = new();
    
    // Business
    public decimal BaseHourlyRate { get; set; }
    public Dictionary<string, decimal> HourlyRateByType { get; set; } = new();
    public bool Is24Hours { get; set; }
    public TimeSpan? OpenTime { get; set; }
    public TimeSpan? CloseTime { get; set; }
    public ICollection<string> Images { get; set; } = new List<string>();

    // Contact
    public string ContactPhone { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty; 
    public string Website { get; set; } = string.Empty;

    // Features
    public bool IsActive { get; set; }
    public bool HasCamera { get; set; }
    public bool HasRoof { get; set; }
    public bool HasOvernightParking { get; set; }
    public bool HasDisabledAccess { get; set; }
    public bool HasWashing { get; set; }
    public bool HasMaintenance { get; set; }

    // Rating
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }

    // Ownership & Timestamps
    public string OwnerId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}