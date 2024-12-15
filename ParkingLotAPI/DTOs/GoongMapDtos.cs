namespace ParkingLotAPI.DTOs
{
    public class PlaceDetailDto
    {
        public string PlaceId { get; set; } = string.Empty;
        public string FormattedAddress { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public GoongGeometry Geometry { get; set; } = new();
        public GoongCompound Compound { get; set; } = new();
        public string[] Types { get; set; } = Array.Empty<string>();
        public PlusCode PlusCode { get; set; } = new();
        public string[] Terms { get; set; } = Array.Empty<string>();
        public bool HasChildren { get; set; }
        public double? Rating { get; set; }
        public string Reference { get; set; } = string.Empty;
    }

    public class GeocodingResultDto
    {
        public string FormattedAddress { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Commune { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public PlusCode PlusCode { get; set; } = new();
        public GoongCompound Compound { get; set; } = new();
        public string[] Types { get; set; } = Array.Empty<string>();
        public Term[] Terms { get; set; } = Array.Empty<Term>();
        public bool HasChildren { get; set; }
    }

    public class AutocompleteResultDto
    {
        public string Status { get; set; } = string.Empty;
        public List<PredictionDto> Predictions { get; set; } = new();
    }

    public class PredictionDto
    {
        public string Description { get; set; } = string.Empty;
        public string PlaceId { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public StructuredFormatting StructuredFormatting { get; set; } = new();
        public bool HasChildren { get; set; }
        public PlusCode PlusCode { get; set; } = new();
        public GoongCompound Compound { get; set; } = new();
        public Term[] Terms { get; set; } = Array.Empty<Term>();
        public string[] Types { get; set; } = Array.Empty<string>();
        public int? DistanceMeters { get; set; }
    }

    public class StructuredFormatting
    {
        public string Main_text { get; set; } = string.Empty;
        public List<MatchedSubstring> Main_text_matched_substrings { get; set; } = new();
        public string Secondary_text { get; set; } = string.Empty;
        public List<MatchedSubstring> Secondary_text_matched_substrings { get; set; } = new();
    }

    public class TextMatch
    {
        public int Length { get; set; }
        public int Offset { get; set; }
    }

    public class Term
    {
        public int Offset { get; set; }
        public string Value { get; set; } = string.Empty;
    }

    public class PlusCode
    {
        public string CompoundCode { get; set; } = string.Empty;
        public string GlobalCode { get; set; } = string.Empty;
    }
} 