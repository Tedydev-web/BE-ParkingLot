namespace ParkingLotAPI.DTOs
{
    public class GoongMapDto
    {
        public GoongResult Result { get; set; } = new GoongResult();
        public string Status { get; set; } = string.Empty;
    }

    public class GoongResult
    {
        public string Place_id { get; set; } = string.Empty;
        public string Formatted_address { get; set; } = string.Empty;
        public GoongGeometry Geometry { get; set; } = new GoongGeometry();
        public GoongCompound? Compound { get; set; }
        public string[] Types { get; set; } = Array.Empty<string>();
    }

    public class GoongGeometry
    {
        public GoongLocation Location { get; set; } = new GoongLocation();
    }

    public class GoongLocation
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    public class GoongCompound
    {
        public string? District { get; set; }
        public string? Commune { get; set; }
        public string? Province { get; set; }
    }
} 