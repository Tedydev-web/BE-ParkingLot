using ParkingLot.Core.Entities;

namespace ParkingLot.Core.DTOs;

public interface INewParkingLotDto
{
    string Name { get; set; }
    string Description { get; set; }
    string LocationId { get; set; }
    string Address { get; set; }
    double Latitude { get; set; }
    double Longitude { get; set; }
    string PhoneNumber { get; set; }
    string Email { get; set; }
    Dictionary<string, int> SlotTypes { get; set; }
    Dictionary<string, decimal> PricesByVehicleType { get; set; }
    bool HasCamera { get; set; }
    bool HasRoof { get; set; }
    bool HasOvernightParking { get; set; }
    bool HasDisabledAccess { get; set; }
}
