public class ParkingLotDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public LocationDto Location { get; set; } = new LocationDto();
    public int TotalSpaces { get; set; }
    public int? AvailableSpaces { get; set; }
    public decimal PricePerHour { get; set; }
    public string? OpeningTime { get; set; }
    public string? ClosingTime { get; set; }
    public bool IsOpen24Hours { get; set; }
    public List<string>? Images { get; set; }
    public double? Rating { get; set; }
    public string? Description { get; set; }
    public string? ContactNumber { get; set; }
} 