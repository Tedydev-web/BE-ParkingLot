namespace ParkingLot.Core.Entities;

public class ParkingLot
{
    public ParkingLot()
    {
        Name = string.Empty;
        Description = string.Empty;
        LocationId = string.Empty;
        Location = new Location();
        SlotTypeCapacity = new Dictionary<string, int>();
        HourlyRateByType = new Dictionary<string, decimal>();
        ContactPhone = string.Empty;
        ContactEmail = string.Empty;
        Website = string.Empty;
        OwnerId = string.Empty;
        Owner = new ApplicationUser();
        Reviews = new List<Review>();
        ParkingSlots = new List<ParkingSlot>();
        PriceHistory = new List<PriceHistory>();
        Images = new List<string>();
    }

    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public string Description { get; set; }
    
    // Location Information
    public string LocationId { get; set; }
    public Location Location { get; set; }
    
    // Capacity Information
    public int TotalSlots { get; set; }
    public int AvailableSlots { get; set; }
    public Dictionary<string, int> SlotTypeCapacity { get; set; } // Số lượng slot cho mỗi loại xe
    
    // Business Information
    public decimal BaseHourlyRate { get; set; }
    public Dictionary<string, decimal> HourlyRateByType { get; set; } // Giá theo loại xe
    public bool IsActive { get; set; } = true;
    public bool Is24Hours { get; set; }
    public TimeSpan? OpenTime { get; set; }
    public TimeSpan? CloseTime { get; set; }
    public ICollection<string> Images { get; set; } = new List<string>(); // URL của ảnh
    
    // Contact Information
    public string ContactPhone { get; set; }
    public string ContactEmail { get; set; }
    public string Website { get; set; }
    
    // Features & Amenities
    public bool HasCamera { get; set; }
    public bool HasRoof { get; set; }
    public bool HasOvernightParking { get; set; }
    public bool HasDisabledAccess { get; set; }
    public bool HasWashing { get; set; }
    public bool HasMaintenance { get; set; }
    
    // Rating Information
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    
    // Ownership & Timestamps
    public string OwnerId { get; set; }
    public ApplicationUser Owner { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation Properties
    public ICollection<Review> Reviews { get; set; }
    public ICollection<ParkingSlot> ParkingSlots { get; set; }
    public ICollection<PriceHistory> PriceHistory { get; set; } = new List<PriceHistory>();
}