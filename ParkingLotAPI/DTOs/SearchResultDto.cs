public class SearchResultDto
{
    public string Status { get; set; } = "OK";
    public string Message { get; set; } = string.Empty;
    public List<ParkingLotResponseDto> Results { get; set; } = new List<ParkingLotResponseDto>();
    public SearchMetadata Metadata { get; set; } = new SearchMetadata
    {
        Total = 0,
        Limit = 10
    };
}

public class SearchMetadata
{
    public int Total { get; set; }
    public int Limit { get; set; }
} 