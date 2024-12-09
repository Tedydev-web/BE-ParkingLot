using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingLot.Application.DTOs;
using ParkingLot.Core.Interfaces;
using System.Security.Claims;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Cors;

namespace ParkingLot.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [EnableCors]  // Change this to use default policy
    [Tags("Parking Lots")]
    public class ParkingLotController : ControllerBase
    {
        private readonly IParkingLotService _parkingLotService;
        private readonly ILogger<ParkingLotController> _logger;

        public ParkingLotController(
            IParkingLotService parkingLotService,
            ILogger<ParkingLotController> logger)
        {
            _parkingLotService = parkingLotService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new parking lot
        /// </summary>
        /// <param name="dto">The parking lot creation details</param>
        /// <returns>The newly created parking lot</returns>
        /// <response code="201">Returns the newly created parking lot</response>
        /// <response code="400">If the item is invalid</response>
        /// <response code="401">If the user is not authorized</response>
        /// <response code="403">If the user is not an admin</response>
        [HttpPost]
        [Authorize(Roles = "ADMIN")] // Make sure this matches the exact role name in your token
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ParkingLotDto>> Create([FromBody] CreateParkingLotDto dto)
        {
            try 
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return Unauthorized();

                var parkingLot = await _parkingLotService.CreateAsync(dto, userId);
                return CreatedAtAction(nameof(GetById), new { id = parkingLot.Id }, parkingLot);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating parking lot: {ex}");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets all parking lots with optional filtering
        /// </summary>
        /// <param name="filter">Filter parameters</param>
        /// <returns>List of parking lots</returns>
        [HttpGet]
        [AllowAnonymous]
        [EnableCors]  // Keep this for the specific endpoint
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ParkingLotDto>>> GetAll([FromQuery] ParkingLotFilterDto filter)
        {
            try 
            {
                // Add CORS headers manually if needed
                Response.Headers.Add("Access-Control-Allow-Origin", "*");
                Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

                var parkingLots = await _parkingLotService.GetAllAsync();
                return Ok(parkingLots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving parking lots");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Gets a specific parking lot by id
        /// </summary>
        /// <param name="id">The parking lot id</param>
        /// <returns>The requested parking lot</returns>
        /// <response code="200">Returns the requested parking lot</response>
        /// <response code="404">If the parking lot is not found</response>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ParkingLotDto>> GetById(string id)
        {
            var parkingLot = await _parkingLotService.GetByIdAsync(id);
            if (parkingLot == null) return NotFound();
            return Ok(parkingLot);
        }

        /// <summary>
        /// Updates a specific parking lot
        /// </summary>
        /// <param name="id">The parking lot id</param>
        /// <param name="dto">The update details</param>
        /// <returns>The updated parking lot</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ParkingLotDto>> Update(string id, [FromBody] UpdateParkingLotDto dto)
        {
            try
            {
                var parkingLot = await _parkingLotService.UpdateAsync(id, dto);
                return Ok(parkingLot);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating parking lot: {ex}");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Deletes a specific parking lot
        /// </summary>
        /// <param name="id">The parking lot id to delete</param>
        /// <returns>No content on success</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                await _parkingLotService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting parking lot: {ex}");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets address details from coordinates
        /// </summary>
        /// <param name="lat">Latitude</param>
        /// <param name="lng">Longitude</param>
        /// <returns>Address details from the coordinates</returns>
        [HttpGet("geocode")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAddressFromCoordinates([FromQuery] double lat, [FromQuery] double lng)
        {
            var client = new HttpClient();
            var response = await client.GetAsync($"https://rsapi.goong.io/Geocode?latlng={lat},{lng}&api_key=WlmIT8XtdGdBS6pBOePEve49zUx9waRQDSOXrVRv");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Ok(content);
            }

            return BadRequest();
        }

        /// <summary>
        /// Searches for places based on keyword and optional coordinates
        /// </summary>
        /// <param name="keyword">Search term</param>
        /// <param name="lat">Optional latitude</param>
        /// <param name="lng">Optional longitude</param>
        /// <returns>Search results</returns>
        [HttpGet("search")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SearchPlaces([FromQuery] string keyword, [FromQuery] double? lat = null, [FromQuery] double? lng = null)
        {
            var client = new HttpClient();
            var url = $"https://rsapi.goong.io/Place/AutoComplete?api_key=WlmIT8XtdGdBS6pBOePEve49zUx9waRQDSOXrVRv&input={Uri.EscapeDataString(keyword)}";
            
            if (lat.HasValue && lng.HasValue)
            {
                url += $"&location={lat},{lng}";
            }

            var response = await client.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Ok(content);
            }

            return BadRequest();
        }
    }
}