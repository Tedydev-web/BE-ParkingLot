// Models/ParkingLot.cs
namespace ParkingLotAPI.Models
{
    public class ParkingLot
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Place_id { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int TotalSpaces { get; set; }
        public int AvailableSpaces { get; set; }
        public decimal PricePerHour { get; set; }
        public TimeSpan? OpeningTime { get; set; }
        public TimeSpan? ClosingTime { get; set; }
        public bool IsOpen24Hours { get; set; }
        public double Rating { get; set; }
        public string Types { get; set; } = "parking";
        public ICollection<ParkingLotImage> Images { get; set; } = new List<ParkingLotImage>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
    }
}