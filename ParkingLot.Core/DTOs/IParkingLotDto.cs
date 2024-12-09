namespace ParkingLot.Core.DTOs;

public interface IParkingLotDto
{
    string Id { get; set; }
    string Name { get; set; }
    string Description { get; set; }
    
    // Location
    string LocationId { get; set; }
    string Address { get; set; }
    double Latitude { get; set; }
    double Longitude { get; set; }
    
    // Contact
    string ContactPhone { get; set; }
    string ContactEmail { get; set; }
    string Website { get; set; }

    // Features
    bool IsActive { get; set; }
    bool HasCamera { get; set; }
    bool HasRoof { get; set; }
    bool HasOvernightParking { get; set; }
    bool HasDisabledAccess { get; set; }
    bool HasWashing { get; set; }
    bool HasMaintenance { get; set; }
    
    // Business
    decimal BaseHourlyRate { get; set; }
    bool Is24Hours { get; set; }
    TimeSpan? OpenTime { get; set; }
    TimeSpan? CloseTime { get; set; }

    // Common
    string OwnerId { get; set; }
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
    string ContactPhone { get; set; }
    string ContactEmail { get; set; }
    string Website { get; set; }
    decimal BaseHourlyRate { get; set; }
    bool Is24Hours { get; set; }
    TimeSpan? OpenTime { get; set; }
    TimeSpan? CloseTime { get; set; }
    bool HasCamera { get; set; }
    bool HasRoof { get; set; }
    bool HasOvernightParking { get; set; }
    bool HasDisabledAccess { get; set; }
    bool HasWashing { get; set; }
    bool HasMaintenance { get; set; }
    Dictionary<string, int> SlotTypes { get; set; }
    Dictionary<string, decimal> PricesByVehicleType { get; set; }
    ICollection<string> Images { get; set; }
}

public interface IUpdateParkingLotDto
{
    string Name { get; set; }
    string Description { get; set; }
    string ContactPhone { get; set; }
    string ContactEmail { get; set; }
    bool HasCamera { get; set; }
    bool HasRoof { get; set; }
    bool HasOvernightParking { get; set; }
    bool HasDisabledAccess { get; set; }
    bool IsActive { get; set; }
}