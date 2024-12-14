// Models/ParkingLot.cs
namespace ParkingLotAPI.Models
{
    public class ParkingLot
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Place_id { get; set; }
        public string Reference { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int TotalSpaces { get; set; }
        public int AvailableSpaces { get; set; }
        public decimal PricePerHour { get; set; }
        public TimeSpan? OpeningTime { get; set; } = new TimeSpan(23, 59, 59);
        public TimeSpan? ClosingTime { get; set; } = new TimeSpan(23, 59, 59);
        public bool IsOpen24Hours { get; set; }
        public List<ParkingLotImage> Images { get; set; }
        public double Rating { get; set; }
        public string Description { get; set; }
        public string ContactNumber { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}