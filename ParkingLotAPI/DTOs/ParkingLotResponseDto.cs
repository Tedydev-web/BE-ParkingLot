public class ParkingLotResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Place_id { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Formatted_address { get; set; } = string.Empty;
    public Geometry? Geometry { get; set; }
    public string[] Types { get; set; } = new[] { "parking" };
    public double Rating { get; set; }
    public OpeningHours? Opening_hours { get; set; }
    public List<Photo>? Photos { get; set; }
    public string? Formatted_phone_number { get; set; }
    public int? Total_spaces { get; set; }
    public int? Available_spaces { get; set; }
    public decimal? Price_per_hour { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class Geometry
{
    public Location Location { get; set; } = new Location();
}

public class Location
{
    public double Lat { get; set; }
    public double Lng { get; set; }
}

public class OpeningHours
{
    public bool Open_now { get; set; }
    public string[] Weekday_text { get; set; } = Array.Empty<string>();
}

public class Photo
{
    public string Photo_reference { get; set; } = string.Empty;
    public bool IsMain { get; set; }
    public DateTime CreatedAt { get; set; }
} 