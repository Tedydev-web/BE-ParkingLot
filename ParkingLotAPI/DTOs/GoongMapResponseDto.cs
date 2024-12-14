public class GoongMapResponseDto
{
    public string Status { get; set; } = "OK";
    public List<ParkingLotDto> Results { get; set; } = new List<ParkingLotDto>();
    public string Message { get; set; } = string.Empty;
    public GoongMapMetadata Metadata { get; set; } = new GoongMapMetadata();
}

public class GoongMapMetadata
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
} 