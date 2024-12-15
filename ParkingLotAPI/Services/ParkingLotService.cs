using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using ParkingLotAPI.Data;
using ParkingLotAPI.DTOs;
using ParkingLotAPI.Models;
using ParkingLotAPI.Services;

namespace ParkingLotAPI.Services
{
    public class ParkingLotService : IParkingLotService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ParkingLotService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IGoongMapService _goongMapService;

        public ParkingLotService(
            ApplicationDbContext context,
            IMapper mapper,
            IWebHostEnvironment environment,
            ILogger<ParkingLotService> logger,
            IConfiguration configuration,
            IGoongMapService goongMapService)
        {
            _context = context;
            _mapper = mapper;
            _environment = environment;
            _logger = logger;
            _configuration = configuration;
            _goongMapService = goongMapService;
        }

        public async Task<SearchResultDto> SearchParkingLots(double latitude, double longitude, int radius)
        {
            try 
            {
                // Lấy danh sách bãi đỗ xe trong bán kính
                var parkingLots = await _context.ParkingLots
                    .Include(p => p.Images)
                    .AsNoTracking()
                    .ToListAsync();

                // Tính toán và lọc theo khoảng cách
                var nearbyParkingLots = parkingLots
                    .Select(p => new
                    {
                        ParkingLot = p,
                        Distance = CalculateDistance(latitude, longitude, p.Latitude, p.Longitude)
                    })
                    .Where(x => x.Distance <= radius)
                    .OrderBy(x => x.Distance)
                    .ToList();

                if (!nearbyParkingLots.Any())
                {
                    return new SearchResultDto
                    {
                        Status = "ZERO_RESULTS",
                        Message = "Không tìm thấy bãi đỗ xe nào trong khu vực",
                        Results = new List<ParkingLotResponseDto>(),
                        Metadata = new SearchMetadata
                        {
                            Total = 0,
                            Limit = 0
                        }
                    };
                }

                // Lấy base URL từ configuration
                var baseUrl = _configuration.GetValue<string>("BaseUrl") ?? "http://localhost:8000";
                baseUrl = baseUrl.TrimEnd('/');
                
                var results = nearbyParkingLots.Select(x => new ParkingLotResponseDto
                {
                    Id = x.ParkingLot.Id,
                    Place_id = x.ParkingLot.Place_id,
                    Reference = x.ParkingLot.Reference,
                    Name = x.ParkingLot.Name,
                    Formatted_address = x.ParkingLot.Address,
                    Geometry = new Geometry
                    {
                        Location = new Location
                        {
                            Lat = x.ParkingLot.Latitude,
                            Lng = x.ParkingLot.Longitude
                        }
                    },
                    Types = new[] { "parking" },
                    Rating = x.ParkingLot.Rating,
                    Opening_hours = new OpeningHours
                    {
                        Open_now = x.ParkingLot.IsOpen24Hours || IsOpenNow(x.ParkingLot.OpeningTime, x.ParkingLot.ClosingTime, x.ParkingLot.IsOpen24Hours),
                        Weekday_text = new[]
                        {
                            $"Giờ mở cửa: {(x.ParkingLot.IsOpen24Hours ? "24/7" : $"{x.ParkingLot.OpeningTime:hh\\:mm} - {x.ParkingLot.ClosingTime:hh\\:mm}")}"
                        }
                    },
                    Photos = x.ParkingLot.Images?
                        .OrderByDescending(img => img.IsMain)
                        .ThenBy(img => img.CreatedAt)
                        .Select(img => new Photo
                        {
                            Photo_reference = $"{baseUrl}{img.ImageUrl}",
                            IsMain = img.IsMain,
                            CreatedAt = img.CreatedAt
                        })
                        .ToList() ?? new List<Photo>(),
                    Formatted_phone_number = x.ParkingLot.ContactNumber,
                    Total_spaces = x.ParkingLot.TotalSpaces,
                    Available_spaces = x.ParkingLot.AvailableSpaces,
                    Price_per_hour = x.ParkingLot.PricePerHour,
                    Description = x.ParkingLot.Description,
                    IsOpen24Hours = x.ParkingLot.IsOpen24Hours,
                    CreatedAt = x.ParkingLot.CreatedAt,
                    UpdatedAt = x.ParkingLot.UpdatedAt,
                    Distance = Math.Round(x.Distance, 2)
                }).ToList();

                return new SearchResultDto
                {
                    Status = "OK",
                    Message = $"Tìm thấy {results.Count} bãi đỗ xe trong bán kính {radius}m",
                    Results = results,
                    Metadata = new SearchMetadata
                    {
                        Total = results.Count,
                        Limit = results.Count,
                        Extra = new Dictionary<string, object>
                        {
                            { "radius", radius },
                            { "center", new { latitude, longitude } },
                            { "min_distance", results.Min(r => r.Distance) },
                            { "max_distance", results.Max(r => r.Distance) }
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm bãi đỗ xe: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<ParkingLotResponseDto> GetParkingLotById(string id)
        {
            try
            {
                var parkingLot = await _context.ParkingLots
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (parkingLot == null)
                    return null;

                var baseUrl = $"{_configuration["BaseUrl"]}".TrimEnd('/');

                return new ParkingLotResponseDto
                {
                    Id = parkingLot.Id,
                    Place_id = parkingLot.Place_id,
                    Reference = parkingLot.Reference,
                    Name = parkingLot.Name,
                    Formatted_address = parkingLot.Address,
                    Geometry = new Geometry
                    {
                        Location = new Location
                        {
                            Lat = parkingLot.Latitude,
                            Lng = parkingLot.Longitude
                        }
                    },
                    Types = parkingLot.Types?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? new[] { "parking" },
                    Rating = parkingLot.Rating,
                    Opening_hours = BuildOpeningHours(parkingLot),
                    Photos = parkingLot.Images?
                        .OrderByDescending(img => img.IsMain)
                        .ThenBy(img => img.CreatedAt)
                        .Select(img => new Photo
                        {
                            Photo_reference = $"{baseUrl}{img.ImageUrl}",
                            IsMain = img.IsMain,
                            CreatedAt = img.CreatedAt
                        })
                        .ToList() ?? new List<Photo>(),
                    Formatted_phone_number = parkingLot.ContactNumber,
                    Total_spaces = parkingLot.TotalSpaces,
                    Available_spaces = parkingLot.AvailableSpaces,
                    Price_per_hour = parkingLot.PricePerHour,
                    Description = parkingLot.Description,
                    IsOpen24Hours = parkingLot.IsOpen24Hours,
                    CreatedAt = parkingLot.CreatedAt,
                    UpdatedAt = parkingLot.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin bãi đỗ xe {Id}: {Message}", id, ex.Message);
                throw;
            }
        }

        public async Task<ParkingLotResponseDto> CreateParkingLot(CreateParkingLotDto dto)
        {
            var parkingLot = new ParkingLot
            {
                Id = Guid.NewGuid().ToString(),
                Place_id = dto.Place_id,
                Reference = dto.Place_id,
                Name = dto.Name,
                Address = dto.Formatted_address,
                Latitude = dto.Geometry.Location.Lat,
                Longitude = dto.Geometry.Location.Lng,
                TotalSpaces = dto.TotalSpaces,
                AvailableSpaces = dto.AvailableSpaces,
                PricePerHour = dto.PricePerHour,
                OpeningTime = dto.IsOpen24Hours ? 
                    TimeSpan.FromHours(0) : 
                    ParseTimeString(dto.OpeningTime),
                ClosingTime = dto.IsOpen24Hours ? 
                    TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)) : 
                    ParseTimeString(dto.ClosingTime),
                IsOpen24Hours = dto.IsOpen24Hours,
                Description = string.IsNullOrEmpty(dto.Description)
                    ? $"Bãi đỗ xe tại {dto.Formatted_address}"
                    : dto.Description,
                ContactNumber = dto.ContactNumber ?? string.Empty,
                Types = "parking",
                Images = new List<ParkingLotImage>()
            };

            // Lưu ParkingLot trước để có Id
            _context.ParkingLots.Add(parkingLot);
            await _context.SaveChangesAsync();

            // Xử lý upload ảnh
            if (dto.Images != null && dto.Images.Any())
            {
                var parkingLotImages = await HandleImageUploads(dto.Images.ToList(), parkingLot.Id);
                parkingLot.Images = parkingLotImages;
                await _context.SaveChangesAsync();
            }

            return await GetParkingLotById(parkingLot.Id);
        }

        private bool IsOpenNow(TimeSpan? openingTime, TimeSpan? closingTime, bool isOpen24Hours)
        {
            if (isOpen24Hours)
                return true;

            if (!openingTime.HasValue || !closingTime.HasValue)
                return false;
            
            var now = DateTime.Now.TimeOfDay;
            
            // Xử lý trường hợp qua ngày
            if (closingTime.Value < openingTime.Value)
            {
                return now >= openingTime.Value || now <= closingTime.Value;
            }

            return now >= openingTime.Value && now <= closingTime.Value;
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in kilometers

            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c;

            return d * 1000; // Convert to meters
        }

        private double ToRad(double value)
        {
            return value * Math.PI / 180;
        }

        public async Task<List<ParkingLotDto>> SearchByKeyword(string keyword)
        {
            var parkingLots = await _context.ParkingLots
                .Include(p => p.Images)
                .Where(p => p.Name.Contains(keyword) || p.Address.Contains(keyword))
                .ToListAsync();

            return _mapper.Map<List<ParkingLotDto>>(parkingLots);
        }

        public async Task<SearchResultDto> GetAllParkingLots()
        {
            try 
            {
                var parkingLots = await _context.ParkingLots
                    .Include(p => p.Images)
                    .AsNoTracking()
                    .ToListAsync();

                var baseUrl = $"{_configuration["BaseUrl"]}".TrimEnd('/');

                var results = parkingLots.Select(p => new ParkingLotResponseDto
                {
                    Id = p.Id,
                    Place_id = p.Place_id,
                    Reference = p.Reference,
                    Name = p.Name,
                    Formatted_address = p.Address,
                    Geometry = new Geometry
                    {
                        Location = new Location
                        {
                            Lat = p.Latitude,
                            Lng = p.Longitude
                        }
                    },
                    Rating = p.Rating,
                    Types = new[] { "parking" },
                    Opening_hours = new OpeningHours
                    {
                        Open_now = p.IsOpen24Hours || IsOpenNow(p.OpeningTime, p.ClosingTime, p.IsOpen24Hours),
                        Weekday_text = new[]
                        {
                            $"Giờ mở cửa: {(p.IsOpen24Hours ? "24/7" : $"{p.OpeningTime:hh\\:mm} - {p.ClosingTime:hh\\:mm}")}"
                        }
                    },
                    Photos = p.Images?
                        .OrderByDescending(img => img.IsMain)
                        .ThenBy(img => img.CreatedAt)
                        .Select(img => new Photo
                        {
                            Photo_reference = $"{baseUrl}{img.ImageUrl}",
                            IsMain = img.IsMain,
                            CreatedAt = img.CreatedAt
                        })
                        .ToList() ?? new List<Photo>(),
                    Formatted_phone_number = p.ContactNumber,
                    Total_spaces = p.TotalSpaces,
                    Available_spaces = p.AvailableSpaces,
                    Price_per_hour = p.PricePerHour,
                    Description = p.Description,
                    IsOpen24Hours = p.IsOpen24Hours,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList();

                return new SearchResultDto
                {
                    Status = results.Any() ? "OK" : "ZERO_RESULTS",
                    Message = results.Any() ? null : "Không tìm thấy bãi đỗ xe nào",
                    Results = results,
                    Metadata = new SearchMetadata
                    {
                        Total = results.Count,
                        Limit = results.Count
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách bãi đỗ xe: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<ParkingLotResponseDto> UpdateParkingLot(string id, CreateParkingLotDto updateDto)
        {
            var parkingLot = await _context.ParkingLots
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (parkingLot == null)
                throw new KeyNotFoundException("Không tìm thấy bãi đỗ xe");

            // Cập nhật thông tin cơ bản
            parkingLot.Name = updateDto.Name;
            parkingLot.Address = updateDto.Formatted_address;
            parkingLot.Latitude = updateDto.Geometry.Location.Lat;
            parkingLot.Longitude = updateDto.Geometry.Location.Lng;
            parkingLot.TotalSpaces = updateDto.TotalSpaces;
            parkingLot.AvailableSpaces = updateDto.AvailableSpaces;
            parkingLot.PricePerHour = updateDto.PricePerHour;
            parkingLot.OpeningTime = updateDto.IsOpen24Hours ? 
                TimeSpan.FromHours(0) : 
                ParseTimeString(updateDto.OpeningTime);
            parkingLot.ClosingTime = updateDto.IsOpen24Hours ? 
                TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)) : 
                ParseTimeString(updateDto.ClosingTime);
            parkingLot.IsOpen24Hours = updateDto.IsOpen24Hours;
            parkingLot.Description = string.IsNullOrEmpty(updateDto.Description)
                ? $"Bãi đỗ xe tại {updateDto.Formatted_address}"
                : updateDto.Description;
            parkingLot.ContactNumber = updateDto.ContactNumber ?? string.Empty;
            parkingLot.Types = "parking";
            parkingLot.UpdatedAt = DateTime.UtcNow;

            // Xử lý upload ảnh mới
            if (updateDto.Images != null && updateDto.Images.Any())
            {
                var newImages = await HandleImageUploads(updateDto.Images.ToList(), parkingLot.Id);
                foreach (var image in newImages)
                {
                    parkingLot.Images.Add(image);
                }
            }

            await _context.SaveChangesAsync();
            return await GetParkingLotById(id);
        }

        public async Task DeleteParkingLot(string id)
        {
            var parkingLot = await _context.ParkingLots
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (parkingLot == null)
                throw new KeyNotFoundException("Không tìm thấy bãi đỗ xe");

            // Xóa các file ảnh
            if (parkingLot.Images != null && parkingLot.Images.Any())
            {
                foreach (var image in parkingLot.Images)
                {
                    var imagePath = Path.Combine(_environment.WebRootPath, image.ImageUrl.TrimStart('/'));
                    if (File.Exists(imagePath))
                    {
                        File.Delete(imagePath);
                    }
                }
            }

            _context.ParkingLots.Remove(parkingLot);
            await _context.SaveChangesAsync();
        }

        public async Task<SearchResultDto> SearchNearbyParkingLots(double latitude, double longitude, int radius, int limit)
        {
            try
            {
                // Lấy danh sách bãi đỗ xe từ DB và tính khoảng cách
                var parkingLots = await _context.ParkingLots
                    .Include(p => p.Images)
                    .AsNoTracking()
                    .ToListAsync();

                var nearbyParkingLots = parkingLots
                    .Select(p => new
                    {
                        ParkingLot = p,
                        Distance = CalculateDistance(latitude, longitude, p.Latitude, p.Longitude)
                    })
                    .Where(x => x.Distance <= radius)
                    .OrderBy(x => x.Distance)
                    .Take(limit)
                    .ToList();

                if (!nearbyParkingLots.Any())
                {
                    return new SearchResultDto
                    {
                        Status = "ZERO_RESULTS",
                        Message = $"Không tìm thấy bãi đỗ xe nào trong bán kính {radius}m",
                        Results = new List<ParkingLotResponseDto>(),
                        Metadata = new SearchMetadata
                        {
                            Total = 0,
                            Limit = limit,
                            Extra = new Dictionary<string, object>
                            {
                                { "radius", radius },
                                { "center", new { latitude, longitude } }
                            }
                        }
                    };
                }

                // Bổ sung thông tin từ Goong Map cho mỗi parking lot
                var enrichedResults = new List<ParkingLotResponseDto>();
                var baseUrl = _configuration.GetValue<string>("BaseUrl")?.TrimEnd('/') ?? "http://localhost:8000";

                foreach (var item in nearbyParkingLots)
                {
                    var geocodingInfo = await _goongMapService.GetGeocodingInfo(
                        item.ParkingLot.Latitude, 
                        item.ParkingLot.Longitude
                    );

                    var enrichedLot = new ParkingLotResponseDto
                    {
                        Id = item.ParkingLot.Id,
                        Place_id = item.ParkingLot.Place_id,
                        Reference = item.ParkingLot.Reference,
                        Name = item.ParkingLot.Name,
                        Formatted_address = geocodingInfo.FormattedAddress,
                        Geometry = new Geometry
                        {
                            Location = new Location
                            {
                                Lat = item.ParkingLot.Latitude,
                                Lng = item.ParkingLot.Longitude
                            }
                        },
                        Types = geocodingInfo.Types ?? new[] { "parking" },
                        Rating = item.ParkingLot.Rating,
                        Opening_hours = BuildOpeningHours(item.ParkingLot),
                        Photos = item.ParkingLot.Images?
                            .OrderByDescending(img => img.IsMain)
                            .ThenBy(img => img.CreatedAt)
                            .Select(img => new Photo
                            {
                                Photo_reference = $"{baseUrl}{img.ImageUrl}",
                                IsMain = img.IsMain,
                                CreatedAt = img.CreatedAt
                            })
                            .ToList() ?? new List<Photo>(),
                        Formatted_phone_number = item.ParkingLot.ContactNumber,
                        Total_spaces = item.ParkingLot.TotalSpaces,
                        Available_spaces = item.ParkingLot.AvailableSpaces,
                        Price_per_hour = item.ParkingLot.PricePerHour,
                        Description = item.ParkingLot.Description,
                        Distance = Math.Round(item.Distance, 2),
                        Compound = new Compound
                        {
                            District = geocodingInfo.District,
                            Commune = geocodingInfo.Commune,
                            Province = geocodingInfo.Province
                        },
                        Plus_code = new ParkingLotAPI.DTOs.PlusCode
                        {
                            CompoundCode = geocodingInfo.PlusCode.CompoundCode,
                            GlobalCode = geocodingInfo.PlusCode.GlobalCode
                        },
                        Terms = geocodingInfo.Terms?.Select(t => t.Value).ToArray() ?? Array.Empty<string>(),
                        Has_children = geocodingInfo.HasChildren
                    };

                    enrichedResults.Add(enrichedLot);
                }

                return new SearchResultDto
                {
                    Status = "OK",
                    Message = $"Tìm thấy {enrichedResults.Count} bãi đỗ xe trong bán kính {radius}m",
                    Results = enrichedResults,
                    Metadata = new SearchMetadata
                    {
                        Total = enrichedResults.Count,
                        Limit = limit,
                        Extra = new Dictionary<string, object>
                        {
                            { "radius", radius },
                            { "center", new { latitude, longitude } },
                            { "min_distance", enrichedResults.Min(r => r.Distance) },
                            { "max_distance", enrichedResults.Max(r => r.Distance) }
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm bãi đỗ xe gần đây: {Message}", ex.Message);
                throw;
            }
        }

        private TimeSpan? ParseTimeString(string timeString)
        {
            if (string.IsNullOrEmpty(timeString))
                return null;

            return TimeSpan.TryParse(timeString, out TimeSpan time) ? time : null;
        }

        private async Task<List<ParkingLotImage>> HandleImageUploads(List<IFormFile> images, string parkingLotId)
        {
            var parkingLotImages = new List<ParkingLotImage>();
            if (images == null || !images.Any()) return parkingLotImages;

            try
            {
                var parkingLot = await _context.ParkingLots.FindAsync(parkingLotId);
                if (parkingLot == null) throw new Exception("Không tìm thấy thông tin bãi đỗ xe");

                // Tạo thư mục cho từng parking lot
                var parkingLotFolder = Path.Combine(_environment.WebRootPath, "uploads", "parkinglots", parkingLotId);
                Directory.CreateDirectory(parkingLotFolder);

                // Đếm số ảnh hiện có
                var existingImagesCount = await _context.ParkingLotImages
                    .Where(i => i.ParkingLotId == parkingLotId)
                    .CountAsync();

                foreach (var image in images)
                {
                    if (image.Length > 0)
                    {
                        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
                        if (!new[] { ".jpg", ".jpeg", ".png", ".gif" }.Contains(extension))
                            continue;

                        // Format tên file: pl_[id]_[tên]_[main/sub_n]_[timestamp].[ext]
                        var sanitizedName = parkingLot.Name.ToLower()
                            .Replace(" ", "-")
                            .Replace("đ", "d")
                            .Replace("ă", "a")
                            .Replace("â", "a")
                            .Replace("ê", "e")
                            .Replace("ô", "o")
                            .Replace("ơ", "o")
                            .Replace("ư", "u");

                        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                        var imageType = existingImagesCount == 0 ? "main" : $"sub_{existingImagesCount + 1}";
                        var fileName = $"pl_{parkingLotId}_{sanitizedName}_{imageType}_{timestamp}{extension}";

                        var filePath = Path.Combine(parkingLotFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        parkingLotImages.Add(new ParkingLotImage
                        {
                            Id = Guid.NewGuid().ToString(),
                            ParkingLotId = parkingLotId,
                            ImageUrl = $"/uploads/parkinglots/{parkingLotId}/{fileName}",
                            IsMain = existingImagesCount == 0,
                            CreatedAt = DateTime.UtcNow
                        });

                        existingImagesCount++;
                    }
                }

                return parkingLotImages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi upload ảnh cho parking lot {ParkingLotId}", parkingLotId);
                throw;
            }
        }

        private OpeningHours BuildOpeningHours(ParkingLot parkingLot)
        {
            string FormatTime(TimeSpan? time)
            {
                if (parkingLot.IsOpen24Hours)
                    return "24/7";

                if (!time.HasValue)
                    return "Chưa cập nhật";

                try
                {
                    // Format với invariant culture để tránh lỗi locale
                    return time.Value.ToString(@"hh\:mm", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch
                {
                    _logger.LogWarning("Không thể format thời gian {Time} cho parking lot {Id}", 
                        time, parkingLot.Id);
                    return "Chưa cập nhật";
                }
            }

            bool isOpen;
            try
            {
                isOpen = IsOpenNow(parkingLot.OpeningTime, parkingLot.ClosingTime, parkingLot.IsOpen24Hours);
            }
            catch
            {
                _logger.LogWarning("Lỗi khi kiểm tra trạng thái mở cửa cho parking lot {Id}", parkingLot.Id);
                isOpen = false;
            }

            return new OpeningHours
            {
                Open_now = isOpen,
                Operating_hours = new OperatingTime
                {
                    Open = FormatTime(parkingLot.OpeningTime),
                    Close = FormatTime(parkingLot.ClosingTime),
                    Is24Hours = parkingLot.IsOpen24Hours
                }
            };
        }
    }
} 