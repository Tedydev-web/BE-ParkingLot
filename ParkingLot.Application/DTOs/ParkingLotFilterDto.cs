namespace ParkingLot.Application.DTOs
{
    public class ParkingLotFilterDto
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? RadiusInKm { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinCapacity { get; set; }
        public int? MaxCapacity { get; set; }
        public bool? IsAvailable { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }
}
