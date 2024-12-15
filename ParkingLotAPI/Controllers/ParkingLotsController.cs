using Microsoft.AspNetCore.Mvc;
using ParkingLotAPI.DTOs;
using ParkingLotAPI.Services;

namespace ParkingLotAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParkingLotsController : ControllerBase
    {
        private readonly IParkingLotService _parkingLotService;
        private readonly ILogger<ParkingLotsController> _logger;

        public ParkingLotsController(
            IParkingLotService parkingLotService,
            ILogger<ParkingLotsController> logger)
        {
            _parkingLotService = parkingLotService;
            _logger = logger;
        }

        [HttpGet("search")]
        public async Task<ActionResult<SearchResultDto>> Search(
            [FromQuery] double latitude,
            [FromQuery] double longitude,
            [FromQuery] int radius = 1000)
        {
            try
            {
                var result = await _parkingLotService.SearchParkingLots(latitude, longitude, radius);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm bãi đỗ xe");
                return BadRequest(new SearchResultDto 
                { 
                    Status = "ERROR",
                    Message = "Có lỗi xảy ra khi tìm kiếm bãi đỗ xe",
                    Results = new List<ParkingLotResponseDto>()
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ParkingLotResponseDto>> GetById(string id)
        {
            try 
            {
                var parkingLot = await _parkingLotService.GetParkingLotById(id);
                if (parkingLot == null)
                    return NotFound(new SearchResultDto 
                    { 
                        Status = "NOT_FOUND",
                        Message = "Không tìm thấy bãi đỗ xe",
                        Results = new List<ParkingLotResponseDto>()
                    });
                
                return Ok(parkingLot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin bãi đỗ xe");
                return BadRequest(new SearchResultDto 
                { 
                    Status = "ERROR",
                    Message = "Có lỗi xảy ra khi lấy thông tin bãi đỗ xe",
                    Results = new List<ParkingLotResponseDto>()
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ParkingLotResponseDto>> Create([FromForm] CreateParkingLotDto createDto)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (createDto.Geometry?.Location == null)
                {
                    return BadRequest(new { message = "Thiếu thông tin vị trí" });
                }

                // Xử lý thời gian mở cửa
                if (createDto.IsOpen24Hours)
                {
                    createDto.OpeningTime = "00:00";
                    createDto.ClosingTime = "23:59";
                }
                else if (!string.IsNullOrEmpty(createDto.OpeningTime) && !string.IsNullOrEmpty(createDto.ClosingTime))
                {
                    if (!TimeSpan.TryParse(createDto.OpeningTime, out _) || 
                        !TimeSpan.TryParse(createDto.ClosingTime, out _))
                    {
                        return BadRequest(new { message = "Định dạng thời gian không hợp lệ. Vui lòng sử dụng định dạng HH:mm" });
                    }
                }

                // Validate images
                if (createDto.Images != null)
                {
                    foreach (var image in createDto.Images)
                    {
                        if (image.Length > 5 * 1024 * 1024) // 5MB limit
                        {
                            return BadRequest(new { message = "Kích thước ảnh không được vượt quá 5MB" });
                        }

                        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
                        if (!new[] { ".jpg", ".jpeg", ".png", ".gif" }.Contains(extension))
                        {
                            return BadRequest(new { message = "Chỉ chấp nhận file ảnh định dạng jpg, jpeg, png, gif" });
                        }
                    }
                }

                // Đảm bảo các giá trị số không bị null
                createDto.TotalSpaces ??= 0;
                createDto.AvailableSpaces ??= 0;
                createDto.PricePerHour ??= 0;

                // Đảm bảo các trường text không bị null
                createDto.Description ??= $"Bãi đỗ xe tại {createDto.Formatted_address}";
                createDto.ContactNumber ??= string.Empty;

                var result = await _parkingLotService.CreateParkingLot(createDto);
                return CreatedAtAction(
                    nameof(GetById), 
                    new { id = result.Id }, 
                    result
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo bãi đỗ xe mới: {Message}", ex.Message);
                return StatusCode(500, new { 
                    message = "Có lỗi xảy ra khi tạo bãi đỗ xe mới", 
                    error = ex.Message,
                    details = ex.InnerException?.Message 
                });
            }
        }

        [HttpGet]
        public async Task<ActionResult<SearchResultDto>> GetAll()
        {
            try
            {
                var result = await _parkingLotService.GetAllParkingLots();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách bãi đỗ xe");
                return BadRequest(new SearchResultDto 
                { 
                    Status = "ERROR",
                    Message = "Có lỗi xảy ra khi lấy danh sách bãi đỗ xe",
                    Results = new List<ParkingLotResponseDto>()
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ParkingLotResponseDto>> Update(string id, [FromForm] CreateParkingLotDto updateDto)
        {
            try
            {
                var parkingLot = await _parkingLotService.GetParkingLotById(id);
                if (parkingLot == null)
                {
                    return NotFound(new { message = "Không tìm thấy bãi đỗ xe" });
                }

                var result = await _parkingLotService.UpdateParkingLot(id, updateDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật bãi đỗ xe");
                return BadRequest(new { message = "Có lỗi xảy ra khi cập nhật bãi đỗ xe" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                var parkingLot = await _parkingLotService.GetParkingLotById(id);
                if (parkingLot == null)
                {
                    return NotFound(new { message = "Không tìm thấy bãi đỗ xe" });
                }

                await _parkingLotService.DeleteParkingLot(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa bãi đỗ xe");
                return BadRequest(new { message = "Có lỗi xảy ra khi xóa bãi đỗ xe" });
            }
        }

        [HttpGet("nearby")]
        public async Task<ActionResult<SearchResultDto>> GetNearby(
            [FromQuery] string location,
            [FromQuery] int radius = 3000,
            [FromQuery] int limit = 10,
            [FromQuery] bool has_children = false)
        {
            try
            {
                // Parse location string to lat,lng
                var coordinates = location.Split(',');
                if (coordinates.Length != 2 || 
                    !double.TryParse(coordinates[0], out double latitude) ||
                    !double.TryParse(coordinates[1], out double longitude))
                {
                    return BadRequest(new { message = "Invalid location format. Expected format: latitude,longitude" });
                }

                // Validate radius
                if (radius < 1000 || radius > 20000)
                {
                    radius = 3000; // Default to 3km if invalid
                }

                var result = await _parkingLotService.SearchNearbyParkingLots(
                    latitude, 
                    longitude, 
                    radius,
                    limit
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm bãi đỗ xe gần đây");
                return BadRequest(new SearchResultDto 
                { 
                    Status = "ERROR",
                    Message = "Có lỗi xảy ra khi tìm kiếm bãi đỗ xe gần đây",
                    Results = new List<ParkingLotResponseDto>()
                });
            }
        }
    }
} 