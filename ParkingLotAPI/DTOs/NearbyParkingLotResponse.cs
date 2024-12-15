namespace ParkingLotAPI.DTOs
{
    public class NearbyParkingLotResponse
    {
        public List<ParkingLotPrediction> Predictions { get; set; } = new();
        public string Status { get; set; } = "OK";
        public string Message { get; set; } = string.Empty;
    }

    public class ParkingLotPrediction
    {
        public string Description { get; set; } = string.Empty;
        public List<MatchedSubstring> Matched_substrings { get; set; } = new();
        public string Place_id { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public StructuredFormatting Structured_formatting { get; set; } = new();
        public bool Has_children { get; set; }
        public PlusCode Plus_code { get; set; } = new();
        public GoongCompound Compound { get; set; } = new();
        public List<Term> Terms { get; set; } = new();
        public string[] Types { get; set; } = new[] { "parking" };
        public string Name { get; set; } = string.Empty;
        public string Formatted_address { get; set; } = string.Empty;
        public Geometry Geometry { get; set; } = new();
        public double Rating { get; set; }
        public OpeningHours Opening_hours { get; set; } = new();
        public List<Photo> Photos { get; set; } = new();
        public string Formatted_phone_number { get; set; } = string.Empty;
        public int? Total_spaces { get; set; }
        public int? Available_spaces { get; set; }
        public decimal? Price_per_hour { get; set; }
        public bool IsOpen24Hours { get; set; }
        public int Distance_meters { get; set; }
    }

    public class MatchedSubstring
    {
        public int Length { get; set; }
        public int Offset { get; set; }
    }
} 