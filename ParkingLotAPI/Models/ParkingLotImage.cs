    // Models/ParkingLotImage.cs
    namespace ParkingLotAPI.Models
    {
        public class ParkingLotImage
        {
            public string Id { get; set; } = Guid.NewGuid().ToString();
            public string ParkingLotId { get; set; }
            public string ImageUrl { get; set; }
            public bool IsMain { get; set; }
            public ParkingLot ParkingLot { get; set; }
        }
    }