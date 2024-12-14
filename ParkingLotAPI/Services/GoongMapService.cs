using Microsoft.Extensions.Options;
using System.Text.Json;
using ParkingLotAPI.DTOs;
using ParkingLotAPI.Options;
using System;
using System.Threading.Tasks;
using System.Net.Http;

namespace ParkingLotAPI.Services
{
    public class GoongMapService : IGoongMapService
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<GoongApiOptions> _goongOptions;
        private readonly IParkingLotService _parkingLotService;

        public GoongMapService(
            HttpClient httpClient,
            IOptions<GoongApiOptions> goongOptions,
            IParkingLotService parkingLotService)
        {
            _httpClient = httpClient;
            _goongOptions = goongOptions;
            _parkingLotService = parkingLotService;
        }

        public async Task<GoongMapDto> GetLocationInfo(double latitude, double longitude)
        {
            var apiKey = _goongOptions.Value.Key;
            var url = $"https://rsapi.goong.io/Geocode?latlng={latitude},{longitude}&api_key={apiKey}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GoongMapDto>(content);
        }

        public async Task<ParkingLotResponseDto> CreateParkingLotFromLocation(
            double latitude, 
            double longitude, 
            CreateParkingLotDto additionalInfo)
        {
            var locationInfo = await GetLocationInfo(latitude, longitude);

            if (locationInfo.Status != "OK")
            {
                throw new Exception($"Không thể lấy thông tin địa điểm: {locationInfo.Status}");
            }

            var createDto = new CreateParkingLotDto
            {
                Place_id = locationInfo.Result.Place_id,
                Name = additionalInfo.Name ?? locationInfo.Result.Formatted_address,
                Formatted_address = locationInfo.Result.Formatted_address,
                Geometry = new GoongGeometry
                {
                    Location = new GoongLocation
                    {
                        Lat = latitude,
                        Lng = longitude
                    }
                },
            //     Plus_code = locationInfo.Result.Plus_code,
                Compound = locationInfo.Result.Compound,
                Types = locationInfo.Result.Types,
                
                // Thông tin bổ sung từ admin
                TotalSpaces = additionalInfo.TotalSpaces,
                AvailableSpaces = additionalInfo.AvailableSpaces,
                PricePerHour = additionalInfo.PricePerHour,
                OpeningTime = additionalInfo.OpeningTime,
                ClosingTime = additionalInfo.ClosingTime,
                IsOpen24Hours = additionalInfo.IsOpen24Hours,
                Description = additionalInfo.Description ?? 
                    $"Bãi đỗ xe tại {locationInfo.Result.Compound.District}, {locationInfo.Result.Compound.Province}",
                ContactNumber = additionalInfo.ContactNumber,
                Images = additionalInfo.Images
            };

            return await _parkingLotService.CreateParkingLot(createDto);
        }
    }
} 