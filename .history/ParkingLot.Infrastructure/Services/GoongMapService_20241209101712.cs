using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ParkingLot.Core.Entities;
using ParkingLot.Core.Interfaces;
using ParkingLot.Infrastructure.Models;
using GoongMap.SDK;

namespace ParkingLot.Infrastructure.Services;

public class GoongMapService : IGoongMapService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly ILogger<GoongMapService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly GoongMapClient _client;

    public GoongMapService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<GoongMapService> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _apiKey = configuration["GoongMap:ApiKey"] ?? throw new ArgumentNullException("GoongMap:ApiKey");
        _baseUrl = configuration["GoongMap:BaseUrl"] ?? "https://rsapi.goong.io/";
        _logger = logger;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        _client = new GoongMapClient(_apiKey);
    }

    public async Task<Location> GetLocationFromCoordinatesAsync(double lat, double lng)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_baseUrl}Geocode?latlng={lat},{lng}&api_key={_apiKey}";
            
            _logger.LogInformation("Requesting geocode for coordinates: {Lat}, {Lng}", lat, lng);
            
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var geocodeResponse = JsonSerializer.Deserialize<GoongGeocodeResponse>(content, _jsonOptions);

            if (geocodeResponse?.Results == null || geocodeResponse.Results.Length == 0)
            {
                throw new Exception("No results found for the given coordinates");
            }

            return MapGoongResultToLocation(geocodeResponse.Results[0]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting location from coordinates: {Lat}, {Lng}", lat, lng);
            throw;
        }
    }

    public async Task<Location> GetLocationFromAddressAsync(string address)
    {
        if (string.IsNullOrEmpty(address))
        {
            throw new ArgumentNullException(nameof(address));
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var encodedAddress = Uri.EscapeDataString(address);
            var url = $"{_baseUrl}Geocode?address={encodedAddress}&api_key={_apiKey}";
            
            _logger.LogInformation("Requesting geocode for address: {Address}", address);
            
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var geocodeResponse = JsonSerializer.Deserialize<GoongGeocodeResponse>(content, _jsonOptions);

            if (geocodeResponse?.Results == null || geocodeResponse.Results.Length == 0)
            {
                throw new Exception($"No results found for address: {address}");
            }

            return MapGoongResultToLocation(geocodeResponse.Results[0]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting location from address: {Address}", address);
            throw;
        }
    }

    public async Task<IEnumerable<Location>> SearchPlacesAsync(string keyword, double? lat = null, double? lng = null)
    {
        if (string.IsNullOrEmpty(keyword))
        {
            throw new ArgumentNullException(nameof(keyword));
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var encodedKeyword = Uri.EscapeDataString(keyword);
            var url = $"{_baseUrl}Place/AutoComplete?api_key={_apiKey}&input={encodedKeyword}";

            if (lat.HasValue && lng.HasValue)
            {
                url += $"&location={lat.Value},{lng.Value}";
            }

            _logger.LogInformation("Searching places with keyword: {Keyword}", keyword);
            
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonSerializer.Deserialize<GoongPlaceSearchResponse>(content, _jsonOptions);

            if (searchResponse?.Predictions == null)
            {
                return Enumerable.Empty<Location>();
            }

            var locations = new List<Location>();
            foreach (var prediction in searchResponse.Predictions)
            {
                // Lấy chi tiết địa điểm từ PlaceId
                var location = await GetPlaceDetailsAsync(prediction.PlaceId);
                if (location != null)
                {
                    locations.Add(location);
                }
            }

            return locations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching places with keyword: {Keyword}", keyword);
            throw;
        }
    }

    public async Task<double> CalculateDistanceAsync(Location from, Location to)
    {
        if (from == null) throw new ArgumentNullException(nameof(from));
        if (to == null) throw new ArgumentNullException(nameof(to));

        try
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_baseUrl}DistanceMatrix?origins={from.Latitude},{from.Longitude}" +
                     $"&destinations={to.Latitude},{to.Longitude}&api_key={_apiKey}";

            _logger.LogInformation("Calculating distance between points: ({FromLat}, {FromLng}) and ({ToLat}, {ToLng})",
                from.Latitude, from.Longitude, to.Latitude, to.Longitude);
            
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var distanceResponse = JsonSerializer.Deserialize<GoongDistanceMatrixResponse>(content, _jsonOptions);

            if (distanceResponse?.Rows == null || 
                distanceResponse.Rows.Length == 0 || 
                distanceResponse.Rows[0].Elements.Length == 0)
            {
                throw new Exception("No distance information available");
            }

            return distanceResponse.Rows[0].Elements[0].Distance.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating distance between locations");
            throw;
        }
    }

    public async Task<Dictionary<string, double>> GetDistancesAsync(Location origin, IEnumerable<Location> destinations)
    {
        if (origin == null) throw new ArgumentNullException(nameof(origin));
        if (destinations == null) throw new ArgumentNullException(nameof(destinations));

        var destinationsList = destinations.ToList();
        if (!destinationsList.Any())
        {
            return new Dictionary<string, double>();
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var destinationsParam = string.Join("|", destinationsList.Select(d => $"{d.Latitude},{d.Longitude}"));
            var url = $"{_baseUrl}DistanceMatrix?origins={origin.Latitude},{origin.Longitude}" +
                     $"&destinations={destinationsParam}&api_key={_apiKey}";

            _logger.LogInformation("Calculating distances from origin to {Count} destinations", destinationsList.Count);
            
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var distanceResponse = JsonSerializer.Deserialize<GoongDistanceMatrixResponse>(content, _jsonOptions);

            if (distanceResponse?.Rows == null || distanceResponse.Rows.Length == 0)
            {
                throw new Exception("No distance information available");
            }

            var results = new Dictionary<string, double>();
            var elements = distanceResponse.Rows[0].Elements;

            for (var i = 0; i < elements.Length; i++)
            {
                if (elements[i].Status.ToLower() == "ok")
                {
                    results.Add(destinationsList[i].Id, elements[i].Distance.Value);
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating distances to multiple destinations");
            throw;
        }
    }

    private async Task<Location?> GetPlaceDetailsAsync(string placeId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_baseUrl}Place/Detail?place_id={placeId}&api_key={_apiKey}";

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var detailResponse = JsonSerializer.Deserialize<GoongGeocodeResponse>(content, _jsonOptions);

            if (detailResponse?.Results == null || detailResponse.Results.Length == 0)
            {
                return null;
            }

            return MapGoongResultToLocation(detailResponse.Results[0]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting place details for ID: {PlaceId}", placeId);
            return null;
        }
    }

    private static Location MapGoongResultToLocation(GoongResult result)
    {
        var location = new Location
        {
            Latitude = result.Geometry.Location.Lat,
            Longitude = result.Geometry.Location.Lng,
            FormattedAddress = result.FormattedAddress,
            PlaceId = result.PlaceId
        };

        foreach (var component in result.AddressComponents)
        {
            if (component.Types.Contains("street"))
                location.Street = component.LongName;
            else if (component.Types.Contains("ward"))
                location.Ward = component.LongName;
            else if (component.Types.Contains("district"))
                location.District = component.LongName;
            else if (component.Types.Contains("city"))
                location.City = component.LongName;
            else if (component.Types.Contains("province"))
                location.Province = component.LongName;
            else if (component.Types.Contains("postal_code"))
                location.PostalCode = component.LongName;
        }

        return location;
    }

    public async Task<bool> ValidateAddress(string address, double latitude, double longitude)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var encodedAddress = Uri.EscapeDataString(address);
            var url = $"{_baseUrl}Geocode?address={encodedAddress}&api_key={_apiKey}";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return false;

            var content = await response.Content.ReadFromJsonAsync<GoongGeocodeResponse>(_jsonOptions);
            if (content?.Results == null || !content.Results.Any()) return false;

            var result = content.Results.First();
            var resultLat = result.Geometry.Location.Lat;
            var resultLng = result.Geometry.Location.Lng;

            // Allow for small differences in coordinates (approximately 100 meters)
            const double tolerance = 0.001; // roughly 100 meters
            return Math.Abs(latitude - resultLat) < tolerance && 
                   Math.Abs(longitude - resultLng) < tolerance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating address");
            return false;
        }
    }

    public async Task<(double latitude, double longitude)> GetCoordinates(string address)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var encodedAddress = Uri.EscapeDataString(address);
            var url = $"{_baseUrl}Geocode?address={encodedAddress}&api_key={_apiKey}";

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<GoongGeocodeResponse>(_jsonOptions);
            if (content?.Results == null || !content.Results.Any())
                throw new Exception("No results found for the given address");

            var location = content.Results.First().Geometry.Location;
            return (location.Lat, location.Lng);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting coordinates for address: {Address}", address);
            throw;
        }
    }

    public async Task<string?> GetPlaceId(string address)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var encodedAddress = Uri.EscapeDataString(address);
            var url = $"{_baseUrl}Place/AutoComplete?api_key={_apiKey}&input={encodedAddress}";

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<GoongPlaceAutocompleteResponse>(_jsonOptions);
            return content?.Predictions?.FirstOrDefault()?.PlaceId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting place ID for address: {Address}", address);
            return null;
        }
    }

    public async Task<GeocodingResult> GeocodeAsync(string address)
    {
        return await _client.GeocodeAsync(address);
    }
}

public class GoongPlaceAutocompleteResponse
{
    public GoongPrediction[]? Predictions { get; set; }
}

public class GoongPrediction
{
    public string? PlaceId { get; set; }
    public string? Description { get; set; }
}