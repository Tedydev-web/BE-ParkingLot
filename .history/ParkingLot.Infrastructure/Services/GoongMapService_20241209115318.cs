using System.Net.Http;
using System.Threading.Tasks;
using ParkingLot.Core.Entities;
using ParkingLot.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace ParkingLot.Infrastructure.Services
{
    public class GoongMapService : IGoongMapService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly ILogger<GoongMapService> _logger;

        public GoongMapService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<GoongMapService> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _apiKey = configuration["GoongMap:ApiKey"] ?? throw new ArgumentNullException("GoongMap:ApiKey");
            _baseUrl = configuration["GoongMap:BaseUrl"] ?? "https://rsapi.goong.io/";
            _logger = logger;
        }

        public async Task<GeocodingResult> GeocodeAsync(string address)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_baseUrl}geocode?address={Uri.EscapeDataString(address)}&api_key={_apiKey}";
            
            _logger.LogInformation("Gọi API Geocode cho địa chỉ: {Address}", address);
            
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            // Giả sử bạn có lớp GeocodingResult để deserialize phản hồi
            var geocodingResult = JsonSerializer.Deserialize<GeocodingResult>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (geocodingResult == null)
            {
                throw new Exception("Không nhận được kết quả từ API Geocode");
            }

            return geocodingResult;
        }

        // Thêm các phương thức khác tương tự
    }
}