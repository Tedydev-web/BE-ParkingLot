using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using ParkingLotAPI.DTOs;

namespace ParkingLotAPI.Services
{
    public class GoongMapService : IGoongMapService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoongMapService> _logger;

        public GoongMapService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GoongMapService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<CreateParkingLotDto> GetParkingLotInfo(double lat, double lng)
        {
            try
            {
                var goongApiKey = _configuration["GoongApi:Key"];
                var response = await _httpClient.GetAsync(
                    $"https://rsapi.goong.io/Geocode?latlng={lat},{lng}&api_key={goongApiKey}"
                );

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var goongResponse = JsonSerializer.Deserialize<JsonDocument>(content);

                if (!goongResponse.RootElement.TryGetProperty("results", out var results) || 
                    results.GetArrayLength() == 0)
                {
                    return null;
                }

                var firstResult = results[0];
                
                return new CreateParkingLotDto
                {
                    Place_id = firstResult.GetProperty("place_id").GetString() ?? string.Empty,
                    Formatted_address = firstResult.GetProperty("formatted_address").GetString() ?? string.Empty,
                    Geometry = new GoongGeometry
                    {
                        Location = new GoongLocation
                        {
                            Lat = lat,
                            Lng = lng
                        }
                    },
                    Name = firstResult.GetProperty("name").GetString() ?? string.Empty,
                    Description = $"Bãi đỗ xe tại {firstResult.GetProperty("formatted_address").GetString()}",
                    Types = firstResult.GetProperty("types")
                        .EnumerateArray()
                        .Select(t => t.GetString())
                        .Where(t => t != null)
                        .ToArray()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin từ Goong API");
                throw;
            }
        }
    }
} 