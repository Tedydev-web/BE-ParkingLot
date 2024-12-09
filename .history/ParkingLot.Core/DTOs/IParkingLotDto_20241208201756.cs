namespace ParkingLot.Core.DTOs;

public interface IParkingLotDto
{
    string Id { get; set; }
    string Name { get; set; }
    string Description { get; set; }
    string OwnerId { get; set; }
    string Address { get; set; }
    double Latitude { get; set; }
    double Longitude { get; set; }
    string PhoneNumber { get; set; }
    string Email { get; set; }
    bool IsActive { get; set; }
    bool HasCamera { get; set; }
    bool HasRoof { get; set; }
    bool HasOvernightParking { get; set; }
    bool HasDisabledAccess { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}

public interface ICreateParkingLotDto
{
    string Name { get; set; }
    string Description { get; set; }
    string Address { get; set; }
    double Latitude { get; set; }
    double Longitude { get; set; }
    string PhoneNumber { get; set; }
    string Email { get; set; }
    bool HasCamera { get; set; }
    bool HasRoof { get; set; }
    bool HasOvernightParking { get; set; }
    bool HasDisabledAccess { get; set; }
    Dictionary<string, int> SlotTypes { get; set; }
    Dictionary<string, decimal> PricesByVehicleType { get; set; }
}

public interface IUpdateParkingLotDto
{
    string Name { get; set; }
    string Description { get; set; }
    string PhoneNumber { get; set; }
    string Email { get; set; }
    bool HasCamera { get; set; }
    bool HasRoof { get; set; }
    bool HasOvernightParking { get; set; }
    bool HasDisabledAccess { get; set; }
    bool IsActive { get; set; }
} 