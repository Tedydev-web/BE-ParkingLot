namespace ParkingLotAPI.DTOs
{
    public class GoongMapDto
    {
        public GoongResult Result { get; set; }
        public string Status { get; set; }
    }

    public class GoongResult
    {
        public string Place_id { get; set; }
        public string Formatted_address { get; set; }
        public GoongGeometry Geometry { get; set; }
      //   public GoongPlusCode Plus_code { get; set; }
        public GoongCompound Compound { get; set; }
        public string[] Types { get; set; }
    }

    public class GoongGeometry
    {
        public GoongLocation Location { get; set; }
    }

    public class GoongLocation
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

//     public class GoongPlusCode
//     {
//         public string Compound_code { get; set; }
//         public string Global_code { get; set; }
//     }

    public class GoongCompound
    {
        public string District { get; set; }
        public string Commune { get; set; }
        public string Province { get; set; }
    }
} 