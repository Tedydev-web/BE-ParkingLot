using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ParkingLot.API;
using ParkingLot.Application.DTOs;
using ParkingLot.Core.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace ParkingLot.IntegrationTests.Controllers
{
    public class ParkingLotControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ParkingLotControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Configure test database here if needed
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb");
                    });

                    using (var scope = services.BuildServiceProvider().CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        db.Database.EnsureCreated();

                        db.ParkingLots.Add(new Core.Entities.ParkingLot
                        {
                            Id = "1",
                            Name = "Test Parking Lot",
                            Capacity = 100,
                            Location = "Test Location",
                            HourlyRate = 10.00m,
                            CreatedAt = DateTime.UtcNow,
                            OwnerId = "test-owner"
                        });
                        db.SaveChanges();
                    }
                });
            });
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetParkingLots_ReturnsSuccessStatusCode()
        {
            // Arrange
            var request = "/api/parkinglots";

            // Act
            var response = await _client.GetAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            var parkingLots = await response.Content.ReadFromJsonAsync<List<ParkingLotDto>>();
            parkingLots.Should().NotBeNull();
        }

        [Fact]
        public async Task GetParkingLotById_ReturnsSuccessStatusCode()
        {
            // Arrange
            var parkingLotId = "1";
            var request = $"/api/parkinglots/{parkingLotId}";

            // Act
            var response = await _client.GetAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            var parkingLot = await response.Content.ReadFromJsonAsync<ParkingLotDto>();
            parkingLot?.Should().NotBeNull();
            parkingLot?.Id.Should().Be(parkingLotId);
        }

        [Fact]
        public async Task CreateParkingLot_ReturnsCreatedResponse()
        {
            // Arrange
            var newParkingLot = new CreateParkingLotDto
            {
                Name = "Test Parking Lot",
                Capacity = 100,
                HourlyRate = 10.00m,
                Location = "Test Location"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/parkinglots", newParkingLot);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdParkingLot = await response.Content.ReadFromJsonAsync<ParkingLotDto>();
            createdParkingLot?.Should().NotBeNull();
            createdParkingLot?.Name.Should().Be(newParkingLot.Name);
        }

        [Fact]
        public async Task UpdateParkingLot_ReturnsSuccessResponse()
        {
            // Arrange
            var parkingLotId = "1";
            var updateParkingLot = new UpdateParkingLotDto
            {
                Name = "Updated Parking Lot",
                Capacity = 150,
                HourlyRate = 15.00m,
                Location = "Updated Location"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/parkinglots/{parkingLotId}", updateParkingLot);

            // Assert
            response.EnsureSuccessStatusCode();
            var updatedParkingLot = await response.Content.ReadFromJsonAsync<ParkingLotDto>();
            updatedParkingLot?.Should().NotBeNull();
            updatedParkingLot?.Name.Should().Be(updateParkingLot.Name);
        }

        [Fact]
        public async Task DeleteParkingLot_ReturnsNoContent()
        {
            // Arrange
            var parkingLotId = "1";

            // Act
            var response = await _client.DeleteAsync($"/api/parkinglots/{parkingLotId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}