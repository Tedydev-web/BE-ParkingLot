using ParkingLot.Core.DTOs;

namespace ParkingLot.Application.DTOs;

public class CreateParkingLotDto : ICreateParkingLotDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Address { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public bool HasCamera { get; set; }
    public bool HasRoof { get; set; }
    public bool HasOvernightParking { get; set; }
    public bool HasDisabledAccess { get; set; }
    public Dictionary<string, int> SlotTypes { get; set; }
    public Dictionary<string, decimal> PricesByVehicleType { get; set; }
}