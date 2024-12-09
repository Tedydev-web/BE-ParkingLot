using ParkingLot.Core.DTOs;
using ParkingLot.Core.Entities;

namespace ParkingLot.Application.DTOs;

public class ParkingLotDto : IParkingLotDto
{
    public string Id { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool HasCamera { get; set; }
    public bool HasRoof { get; set; }
    public bool HasOvernightParking { get; set; }
    public bool HasDisabledAccess { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}