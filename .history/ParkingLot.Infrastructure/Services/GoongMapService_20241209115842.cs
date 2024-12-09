using System.Net.Http;
using System.Threading.Tasks;
using ParkingLot.Core.Entities;
using ParkingLot.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.Collections.Generic;
using ParkingLot.Core.DTOs;

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

        public async Task<Location> GetLocationFromCoordinatesAsync(double lat, double lng)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_baseUrl}Geocode?latlng={lat},{lng}&api_key={_apiKey}";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var geocodeResponse = JsonSerializer.Deserialize<GoongGeocodeResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (geocodeResponse?.Results == null || geocodeResponse.Results.Length == 0)
            {
                throw new Exception($"No results found for coordinates: {lat}, {lng}");
            }
            return MapGoongResultToLocation(geocodeResponse.Results[0]);
        }

        public async Task<Location> GetLocationFromAddressAsync(string address)
        {
            var client = _httpClientFactory.CreateClient();
            var encodedAddress = Uri.EscapeDataString(address);
            var url = $"{_baseUrl}Geocode?address={encodedAddress}&api_key={_apiKey}";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var geocodeResponse = JsonSerializer.Deserialize<GoongGeocodeResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (geocodeResponse?.Results == null || geocodeResponse.Results.Length == 0)
            {
                throw new Exception($"No results found for address: {address}");
            }
            return MapGoongResultToLocation(geocodeResponse.Results[0]);
        }

        public async Task<IEnumerable<Location>> SearchPlacesAsync(string keyword, double? lat = null, double? lng = null)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_baseUrl}Place/AutoComplete?input={Uri.EscapeDataString(keyword)}&api_key={_apiKey}";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonSerializer.Deserialize<GoongPlaceAutocompleteResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (searchResponse?.Predictions == null || searchResponse.Predictions.Length == 0)
            {
                throw new Exception($"No results found for keyword: {keyword}");
            }
            var locations = new List<Location>();
            foreach (var prediction in searchResponse.Predictions)
            {
                var location = await GetPlaceDetailsAsync(prediction.PlaceId);
                if (location != null)
                {
                    locations.Add(location);
                }
            }
            return locations;
        }

        public async Task<double> CalculateDistanceAsync(Location from, Location to)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_baseUrl}DistanceMatrix?origins={from.Latitude},{from.Longitude}&destinations={to.Latitude},{to.Longitude}&api_key={_apiKey}";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var distanceResponse = JsonSerializer.Deserialize<GoongDistanceMatrixResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (distanceResponse?.Rows == null || distanceResponse.Rows.Length == 0 || distanceResponse.Rows[0].Elements == null || distanceResponse.Rows[0].Elements.Length == 0)
            {
                throw new Exception("No distance data found");
            }
            return distanceResponse.Rows[0].Elements[0].Distance.Value;
        }

        public async Task<Dictionary<string, double>> GetDistancesAsync(Location origin, IEnumerable<Location> destinations)
        {
            var client = _httpClientFactory.CreateClient();
            var destinationString = string.Join("|", destinations.Select(d => $"{d.Latitude},{d.Longitude}"));
            var url = $"{_baseUrl}DistanceMatrix?origins={origin.Latitude},{origin.Longitude}&destinations={destinationString}&api_key={_apiKey}";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var distanceResponse = JsonSerializer.Deserialize<GoongDistanceMatrixResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (distanceResponse?.Rows == null || distanceResponse.Rows.Length == 0 || distanceResponse.Rows[0].Elements == null || distanceResponse.Rows[0].Elements.Length == 0)
            {
                throw new Exception("No distance data found");
            }
            var distances = new Dictionary<string, double>();
            for (int i = 0; i < destinations.Count(); i++)
            {
                distances.Add($"{destinations.ElementAt(i).Latitude},{destinations.ElementAt(i).Longitude}", distanceResponse.Rows[0].Elements[i].Distance.Value);
            }
            return distances;
        }

        public async Task<bool> ValidateAddress(string address, double latitude, double longitude)
        {
            var location = await GetLocationFromAddressAsync(address);
            return location.Latitude == latitude && location.Longitude == longitude;
        }

        public async Task<(double latitude, double longitude)> GetCoordinates(string address)
        {
            var location = await GetLocationFromAddressAsync(address);
            return (location.Latitude, location.Longitude);
        }

        public async Task<string?> GetPlaceId(string address)
        {
            var location = await GetLocationFromAddressAsync(address);
            return location.PlaceId;
        }

        private async Task<Location?> GetPlaceDetailsAsync(string placeId)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_baseUrl}Place/Detail?place_id={placeId}&api_key={_apiKey}";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var detailResponse = JsonSerializer.Deserialize<GoongGeocodeResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (detailResponse?.Results == null || detailResponse.Results.Length == 0)
            {
                return null;
            }
            return MapGoongResultToLocation(detailResponse.Results[0]);
        }

        private static Location MapGoongResultToLocation(GoongResult result)
        {
            return new Location
            {
                Latitude = result.Geometry.Location.Lat,
                Longitude = result.Geometry.Location.Lng,
                PlaceId = result.PlaceId,
                Address = result.FormattedAddress
            };
        }
    }
}