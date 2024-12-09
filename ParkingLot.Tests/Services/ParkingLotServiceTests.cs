using Moq;
using Xunit;
using FluentAssertions;
using ParkingLot.Application.Services;
using ParkingLot.Core.Models;
using ParkingLot.Core.Services;

namespace ParkingLot.Tests.Services
{
    public class ParkingLotServiceTests
    {
        private readonly Mock<IGoongMapService> _mockGoongMapService;
        private readonly IParkingLotService _parkingLotService;

        public ParkingLotServiceTests()
        {
            _mockGoongMapService = new Mock<IGoongMapService>();
            _parkingLotService = new ParkingLotService(_mockGoongMapService.Object);
        }

        [Fact]
        public async Task GetLocationFromAddress_ShouldReturnLocation_WhenAddressIsValid()
        {
            // Arrange
            var address = "123 Test Street";
            var expectedLocation = new Location 
            { 
                Address = address,
                Latitude = 10.0,
                Longitude = 20.0 
            };

            _mockGoongMapService
                .Setup(x => x.GetLocationFromAddressAsync(address))
                .ReturnsAsync(expectedLocation);

            // Act
            var result = await _parkingLotService.GetLocationFromAddress(address);

            // Assert
            result.Should().BeEquivalentTo(expectedLocation);
        }
    }
}