namespace ParkingLot.Core.DTOs
{
    public class GeocodingResult
    {
        public Coordinates Coordinates { get; set; }
        // Add other relevant properties if necessary
    }

    public class Coordinates
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
