public class SearchResultDto
{
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; }
    public List<ParkingLotResponseDto> Results { get; set; } = new();
    public SearchMetadata Metadata { get; set; }
}

public class SearchMetadata
{
    public int Total { get; set; }
    public int Limit { get; set; }
    public Dictionary<string, object> Extra { get; set; } = new();
} 