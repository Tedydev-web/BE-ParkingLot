namespace ParkingLot.Core.Entities;

public class Location
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string FormattedAddress { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Ward { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string Country { get; set; } = "Việt Nam";
    public string PostalCode { get; set; } = string.Empty;
    public string PlaceId { get; set; } = string.Empty; // Goong Map Place ID
    public string Address { get; set; } = string.Empty;
}