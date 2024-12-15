using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ParkingLotAPI.DTOs;

namespace ParkingLotAPI.Services
{
    public class GoongMapService : IGoongMapService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<GoongMapService> _logger;

        public GoongMapService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GoongMapService> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GoongMap:ApiKey"] ?? throw new ArgumentNullException("Goong API key is required");
            _logger = logger;
            _httpClient.BaseAddress = new Uri("https://rsapi.goong.io/");
        }

        public async Task<CreateParkingLotDto> GetPlaceDetails(double lat, double lng)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Geocode?latlng={lat},{lng}&api_key={_apiKey}");
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
                    Description = $"Bãi đỗ xe tại {firstResult.GetProperty("formatted_address").GetString()}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin từ Goong API");
                throw;
            }
        }

        public async Task<GeocodingResultDto> GetGeocodingInfo(double lat, double lng)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Geocode?latlng={lat},{lng}&api_key={_apiKey}");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonDocument.Parse(content).RootElement;

                if (!result.TryGetProperty("results", out var results) || 
                    results.GetArrayLength() == 0)
                {
                    throw new Exception("Không tìm thấy thông tin địa lý");
                }

                var firstResult = results[0];
                
                return new GeocodingResultDto
                {
                    FormattedAddress = firstResult.TryGetProperty("formatted_address", out var address) 
                        ? address.GetString() ?? string.Empty 
                        : string.Empty,
                        
                    District = firstResult.TryGetProperty("compound", out var compound) 
                        ? (compound.TryGetProperty("district", out var district) 
                            ? district.GetString() ?? string.Empty 
                            : string.Empty)
                        : string.Empty,
                        
                    Commune = firstResult.TryGetProperty("compound", out compound) 
                        ? (compound.TryGetProperty("commune", out var commune) 
                            ? commune.GetString() ?? string.Empty 
                            : string.Empty)
                        : string.Empty,
                        
                    Province = firstResult.TryGetProperty("compound", out compound) 
                        ? (compound.TryGetProperty("province", out var province) 
                            ? province.GetString() ?? string.Empty 
                            : string.Empty)
                        : string.Empty,
                        
                    Types = firstResult.TryGetProperty("types", out var types)
                        ? types.EnumerateArray()
                            .Select(t => t.GetString())
                            .Where(t => t != null)
                            .ToArray()
                        : new[] { "parking" },
                        
                    Terms = firstResult.TryGetProperty("terms", out var terms)
                        ? terms.EnumerateArray()
                            .Select(t => new Term 
                            { 
                                Offset = t.TryGetProperty("offset", out var offset) ? offset.GetInt32() : 0,
                                Value = t.TryGetProperty("value", out var value) ? value.GetString() ?? string.Empty : string.Empty
                            })
                            .ToArray()
                        : Array.Empty<Term>(),
                        
                    PlusCode = firstResult.TryGetProperty("plus_code", out var plusCode)
                        ? new ParkingLotAPI.DTOs.PlusCode
                        {
                            CompoundCode = plusCode.TryGetProperty("compound_code", out var compoundCode) 
                                ? compoundCode.GetString() ?? string.Empty 
                                : string.Empty,
                            GlobalCode = plusCode.TryGetProperty("global_code", out var globalCode) 
                                ? globalCode.GetString() ?? string.Empty 
                                : string.Empty
                        }
                        : new ParkingLotAPI.DTOs.PlusCode(),
                        
                    HasChildren = firstResult.TryGetProperty("has_children", out var hasChildren) 
                        ? hasChildren.GetBoolean() 
                        : false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin địa lý từ Goong API: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<PlaceDetailDto> GetPlaceDetail(string placeId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Place/Detail?place_id={placeId}&api_key={_apiKey}");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonDocument.Parse(content).RootElement;
                
                return new PlaceDetailDto
                {
                    PlaceId = placeId,
                    FormattedAddress = result.GetProperty("formatted_address").GetString() ?? string.Empty,
                    Name = result.GetProperty("name").GetString() ?? string.Empty,
                    Reference = result.GetProperty("reference").GetString() ?? string.Empty,
                    // Map các trường khác tương tự
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy chi tiết địa điểm từ Goong API");
                throw;
            }
        }

        public async Task<AutocompleteResultDto> GetPlaceSuggestions(string keyword)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Place/AutoComplete?input={keyword}&api_key={_apiKey}");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonDocument.Parse(content).RootElement;
                
                return new AutocompleteResultDto
                {
                    Status = result.GetProperty("status").GetString() ?? string.Empty,
                    Predictions = result.GetProperty("predictions").EnumerateArray()
                        .Select(p => new PredictionDto
                        {
                            Description = p.GetProperty("description").GetString() ?? string.Empty,
                            PlaceId = p.GetProperty("place_id").GetString() ?? string.Empty,
                            Reference = p.GetProperty("reference").GetString() ?? string.Empty,
                            // Map các trường khác tương tự
                        })
                        .ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy gợi ý địa điểm từ Goong API");
                throw;
            }
        }
    }
} 