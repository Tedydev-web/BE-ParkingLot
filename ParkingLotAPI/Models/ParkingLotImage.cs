    // Models/ParkingLotImage.cs
    namespace ParkingLotAPI.Models
    {
        public class ParkingLotImage
        {
            public string Id { get; set; } = Guid.NewGuid().ToString();
            public string ParkingLotId { get; set; } = string.Empty;
            public string ImageUrl { get; set; } = string.Empty;
            public bool IsMain { get; set; }
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            
            public virtual ParkingLot? ParkingLot { get; set; }
        }
    }