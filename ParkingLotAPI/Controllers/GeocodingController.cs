using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace ParkingLotAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GeocodingController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeocodingController> _logger;

        public GeocodingController(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GeocodingController> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("geocode")]
        public async Task<IActionResult> Geocode([FromQuery] double lat, [FromQuery] double lng)
        {
            try
            {
                var goongApiKey = _configuration["GoongApi:Key"];
                var response = await _httpClient.GetAsync(
                    $"https://rsapi.goong.io/Geocode?latlng={lat},{lng}&api_key={goongApiKey}"
                );

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Goong API Response: {Response}", content);
                
                var goongResponse = JsonSerializer.Deserialize<JsonDocument>(content);

                if (!goongResponse.RootElement.TryGetProperty("results", out var results) || 
                    results.GetArrayLength() == 0)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin địa điểm" });
                }

                var firstResult = results[0];
                string district = string.Empty;
                string commune = string.Empty;
                string province = string.Empty;

                if (firstResult.TryGetProperty("address_components", out var addressComponents))
                {
                    foreach (var component in addressComponents.EnumerateArray())
                    {
                        if (component.TryGetProperty("types", out var componentTypes))
                        {
                            var typeArray = componentTypes.EnumerateArray().Select(t => t.GetString()).ToList();
                            
                            if (typeArray.Contains("administrative_area_level_1") || typeArray.Contains("province"))
                            {
                                province = component.GetProperty("long_name").GetString();
                            }
                            if (typeArray.Contains("administrative_area_level_2") || typeArray.Contains("district"))
                            {
                                district = component.GetProperty("long_name").GetString();
                            }
                            if (typeArray.Contains("administrative_area_level_3") || typeArray.Contains("commune") || typeArray.Contains("ward"))
                            {
                                commune = component.GetProperty("long_name").GetString();
                            }
                        }
                    }
                }

                _logger.LogInformation("Extracted address components - District: {District}, Commune: {Commune}, Province: {Province}", 
                    district, commune, province);

                if (string.IsNullOrEmpty(district) || string.IsNullOrEmpty(commune) || string.IsNullOrEmpty(province))
                {
                    if (firstResult.TryGetProperty("plus_code", out var plusCodeObj) && 
                        plusCodeObj.TryGetProperty("compound_code", out var compoundCodeStr))
                    {
                        var compoundCode = compoundCodeStr.GetString();
                        var parts = compoundCode.Split(',').Select(p => p.Trim()).ToList();
                        
                        if (parts.Count >= 3)
                        {
                            if (string.IsNullOrEmpty(commune))
                            {
                                var communePart = parts[0];
                                var spaceIndex = communePart.IndexOf(' ');
                                commune = spaceIndex >= 0 ? communePart.Substring(spaceIndex + 1) : communePart;
                            }

                            if (string.IsNullOrEmpty(district)) district = parts[1];
                            if (string.IsNullOrEmpty(province)) province = parts[2];
                        }
                    }
                }

                if (firstResult.TryGetProperty("compound", out var compoundObj))
                {
                    if (compoundObj.TryGetProperty("district", out var districtJson) && !string.IsNullOrEmpty(district))
                    {
                        district = districtJson.GetString();
                    }
                    if (compoundObj.TryGetProperty("commune", out var communeJson) && !string.IsNullOrEmpty(commune))
                    {
                        commune = communeJson.GetString();
                    }
                    if (compoundObj.TryGetProperty("province", out var provinceJson) && !string.IsNullOrEmpty(province))
                    {
                        province = provinceJson.GetString();
                    }
                }

                var formattedResponse = new
                {
                    result = new
                    {
                        place_id = firstResult.TryGetProperty("place_id", out var placeId) 
                            ? placeId.GetString() 
                            : string.Empty,
                        formatted_address = firstResult.TryGetProperty("formatted_address", out var address) 
                            ? address.GetString() 
                            : string.Empty,
                        geometry = new
                        {
                            location = new
                            {
                                lat = lat,
                                lng = lng
                            }
                        },
                        // plus_code = firstResult.TryGetProperty("plus_code", out var plusCodeData) 
                        //     ? new
                        //     {
                        //         compound_code = plusCodeData.TryGetProperty("compound_code", out var compoundCodeData) 
                        //             ? compoundCodeData.GetString() 
                        //             : string.Empty,
                        //         global_code = plusCodeData.TryGetProperty("global_code", out var globalCode) 
                        //             ? globalCode.GetString() 
                        //             : string.Empty
                        //     }
                        //     : null,
                        compound = new
                        {
                            district = district,
                            commune = commune,
                            province = province
                        },
                        name = firstResult.TryGetProperty("name", out var name)
                            ? name.GetString()
                            : string.Empty,
                        url = firstResult.TryGetProperty("url", out var url)
                            ? url.GetString()
                            : string.Empty,
                        types = firstResult.TryGetProperty("types", out var resultTypes)
                            ? resultTypes.EnumerateArray()
                                .Select(t => t.GetString())
                                .ToArray()
                            : new[] { "parking" }
                    },
                    status = goongResponse.RootElement.GetProperty("status").GetString()
                };

                return Ok(formattedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi Goong API với lat={Lat}, lng={Lng}", lat, lng);
                return StatusCode(500, new { 
                    message = "Lỗi khi gọi Goong API",
                    error = ex.Message 
                });
            }
        }
    }
} 