using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace ParkingLotAPI.DTOs
{
    public class CreateParkingLotDto
    {
        // Thông tin từ Goong Map
        public string Place_id { get; set; }
        public string Formatted_address { get; set; }
        public GoongGeometry Geometry { get; set; }
      //   public GoongPlusCode Plus_code { get; set; }
        public GoongCompound Compound { get; set; }
        public string Name { get; set; }
        public string? Url { get; set; }
        public string[] Types { get; set; }

        // Thông tin bổ sung từ admin
        public int TotalSpaces { get; set; }
        public int AvailableSpaces { get; set; }
        public decimal PricePerHour { get; set; }
        public TimeSpan? OpeningTime { get; set; }
        public TimeSpan? ClosingTime { get; set; }
        public bool IsOpen24Hours { get; set; }
        public string? Description { get; set; }
        public string? ContactNumber { get; set; }
        public List<IFormFile>? Images { get; set; }
    }
} 