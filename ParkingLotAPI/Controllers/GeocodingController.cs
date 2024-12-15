using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using ParkingLotAPI.Services;

namespace ParkingLotAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GeocodingController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeocodingController> _logger;
        private readonly IGoongMapService _goongMapService;

        public GeocodingController(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GeocodingController> logger,
            IGoongMapService goongMapService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _goongMapService = goongMapService;
        }

        [HttpGet("geocode")]
        public async Task<IActionResult> Geocode([FromQuery] double lat, [FromQuery] double lng)
        {
            try
            {
                var parkingLotInfo = await _goongMapService.GetParkingLotInfo(lat, lng);
                if (parkingLotInfo == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin địa điểm" });
                }

                return Ok(parkingLotInfo);
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