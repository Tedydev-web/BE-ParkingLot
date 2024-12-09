namespace ParkingLot.Infrastructure.Models;

public class GoongGeocodeResponse
{
    public required GoongResult[] Results { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class GoongResult
{
    public string FormattedAddress { get; set; } = string.Empty;
    public GoongGeometry Geometry { get; set; } = new();
    public string PlaceId { get; set; } = string.Empty;
    public GoongAddressComponent[] AddressComponents { get; set; } = Array.Empty<GoongAddressComponent>();
    public string[] Types { get; set; } = Array.Empty<string>();
}

public class GoongGeometry
{
    public GoongLocation Location { get; set; } = new();
    public string LocationType { get; set; } = string.Empty;
    public GoongViewport Viewport { get; set; } = new();
}

public class GoongLocation
{
    public double Lat { get; set; }
    public double Lng { get; set; }
}

public class GoongViewport
{
    public GoongLocation Northeast { get; set; } = new();
    public GoongLocation Southwest { get; set; } = new();
}

public class GoongAddressComponent
{
    public string LongName { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string[] Types { get; set; } = Array.Empty<string>();
}

public class GoongPlaceSearchResponse
{
    public required GoongPrediction[] Predictions { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class GoongPrediction
{
    public required string PlaceId { get; set; }
    public required string Description { get; set; }
    public string[] Types { get; set; } = Array.Empty<string>();
    public GoongStructuredFormatting StructuredFormatting { get; set; } = new();
}

public class GoongStructuredFormatting
{
    public string MainText { get; set; } = string.Empty;
    public string SecondaryText { get; set; } = string.Empty;
}

public class GoongDistanceMatrixResponse
{
    public string Status { get; set; } = string.Empty;
    public required GoongDistanceRow[] Rows { get; set; }
}

public class GoongDistanceRow
{
    public required GoongDistanceElement[] Elements { get; set; }
}

public class GoongDistanceElement
{
    public string Status { get; set; } = string.Empty;
    public required GoongDistance Distance { get; set; }
    public GoongDuration Duration { get; set; } = new();
}

public class GoongDistance
{
    public double Value { get; set; }
    public string Text { get; set; } = string.Empty;
}

public class GoongDuration
{
    public double Value { get; set; }
    public string Text { get; set; } = string.Empty;
} 