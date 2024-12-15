using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace ParkingLotAPI.DTOs
{
    public class CreateParkingLotDto
    {
        // Thông tin từ Goong Map
        public string Place_id { get; set; } = string.Empty;
        public string Formatted_address { get; set; } = string.Empty;
        public GoongGeometry Geometry { get; set; } = new GoongGeometry();
        public string Name { get; set; } = string.Empty;
        public string? Url { get; set; }

        // Thông tin bổ sung từ admin
        public int? TotalSpaces { get; set; }
        public int? AvailableSpaces { get; set; }
        public decimal? PricePerHour { get; set; }
        public string? OpeningTime { get; set; }
        public string? ClosingTime { get; set; }
        public bool IsOpen24Hours { get; set; }
        public string? Description { get; set; }
        public string? ContactNumber { get; set; }
        public List<IFormFile>? Images { get; set; }
    }
} 