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

namespace ParkingLotAPI.Services
{
    public class ParkingLotService : IParkingLotService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment;

        public ParkingLotService(
            ApplicationDbContext context,
            IMapper mapper,
            IWebHostEnvironment environment)
        {
            _context = context;
            _mapper = mapper;
            _environment = environment;
        }

        public async Task<SearchResultDto> SearchParkingLots(double latitude, double longitude, int radius)
        {
            var parkingLots = await _context.ParkingLots
                .Include(p => p.Images)
                .Where(p => CalculateDistance(latitude, longitude, p.Latitude, p.Longitude) <= radius)
                .ToListAsync();

            if (!parkingLots.Any())
            {
                return new SearchResultDto 
                { 
                    Status = "ZERO_RESULTS", 
                    Results = new List<ParkingLotResponseDto>() 
                };
            }

            var results = parkingLots.Select(p => new ParkingLotResponseDto
            {
                Place_id = p.Id,
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
                Opening_hours = new OpeningHours
                {
                    Open_now = p.IsOpen24Hours || IsOpenNow(p.OpeningTime, p.ClosingTime),
                    Weekday_text = new[]
                    {
                        $"Giờ mở cửa: {(p.IsOpen24Hours ? "24/7" : $"{p.OpeningTime} - {p.ClosingTime}")}"
                    }
                },
                Photos = p.Images?.Select(img => new Photo
                {
                    Photo_reference = img.ImageUrl
                }).ToList() ?? new List<Photo>(),
                Formatted_phone_number = p.ContactNumber,
                Total_spaces = p.TotalSpaces,
                Available_spaces = p.AvailableSpaces,
                Price_per_hour = (double)p.PricePerHour
            }).ToList();

            return new SearchResultDto
            {
                Status = "OK",
                Results = results
            };
        }

        public async Task<ParkingLotResponseDto> GetParkingLotById(string id)
        {
            var parkingLot = await _context.ParkingLots
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (parkingLot == null)
                return null;

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
                Rating = parkingLot.Rating,
                Opening_hours = new OpeningHours
                {
                    Open_now = parkingLot.IsOpen24Hours || IsOpenNow(parkingLot.OpeningTime, parkingLot.ClosingTime),
                    Weekday_text = new[]
                    {
                        $"Giờ mở cửa: {(parkingLot.IsOpen24Hours ? "24/7" : $"{parkingLot.OpeningTime} - {parkingLot.ClosingTime}")}"
                    }
                },
                Photos = parkingLot.Images?.Select(img => new Photo
                {
                    Photo_reference = img.ImageUrl
                }).ToList() ?? new List<Photo>(),
                Formatted_phone_number = parkingLot.ContactNumber,
                Total_spaces = parkingLot.TotalSpaces,
                Available_spaces = parkingLot.AvailableSpaces,
                Price_per_hour = (double)parkingLot.PricePerHour,
                Description = parkingLot.Description    
            };
        }

        public async Task<ParkingLotResponseDto> CreateParkingLot(CreateParkingLotDto createDto)
        {
            var parkingLot = new ParkingLot
            {
                Reference = createDto.Place_id,
                Name = createDto.Name,
                Address = createDto.Formatted_address,
                Latitude = createDto.Geometry.Location.Lat,
                Longitude = createDto.Geometry.Location.Lng,
                TotalSpaces = createDto.TotalSpaces,
                AvailableSpaces = createDto.AvailableSpaces,
                PricePerHour = createDto.PricePerHour,
                OpeningTime = createDto.OpeningTime,
                ClosingTime = createDto.ClosingTime,
                IsOpen24Hours = createDto.IsOpen24Hours,
                Description = string.IsNullOrEmpty(createDto.Description)
                    ? $"Bãi đỗ xe tại {createDto.Compound.District}, {createDto.Compound.Province}"
                    : createDto.Description,
                ContactNumber = createDto.ContactNumber,
                Images = new List<ParkingLotImage>()
            };

            parkingLot.Place_id = createDto.Place_id;

            // Xử lý upload hình ảnh
            if (createDto.Images != null && createDto.Images.Any())
            {
                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "parkinglots");
                Directory.CreateDirectory(uploadPath);

                foreach (var image in createDto.Images)
                {
                    if (image.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                        var filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        parkingLot.Images.Add(new ParkingLotImage
                        {
                            ImageUrl = $"/uploads/parkinglots/{fileName}",
                            IsMain = parkingLot.Images.Count == 0 // Ảnh đầu tiên là ảnh chính
                        });
                    }
                }
            }

            _context.ParkingLots.Add(parkingLot);
            await _context.SaveChangesAsync();

            return await GetParkingLotById(parkingLot.Id);
        }

        private bool IsOpenNow(TimeSpan? openingTime, TimeSpan? closingTime)
        {
            if (!openingTime.HasValue || !closingTime.HasValue)
                return false;
            
            var now = DateTime.Now.TimeOfDay;
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
            var parkingLots = await _context.ParkingLots
                .Include(p => p.Images)
                .ToListAsync();

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
                Opening_hours = new OpeningHours
                {
                    Open_now = p.IsOpen24Hours || IsOpenNow(p.OpeningTime, p.ClosingTime),
                    Weekday_text = new[]
                    {
                        $"Giờ mở cửa: {(p.IsOpen24Hours ? "24/7" : $"{p.OpeningTime} - {p.ClosingTime}")}"
                    }
                },
                Photos = p.Images?.Select(img => new Photo
                {
                    Photo_reference = img.ImageUrl
                }).ToList() ?? new List<Photo>(),
                Formatted_phone_number = p.ContactNumber,
                Total_spaces = p.TotalSpaces,
                Available_spaces = p.AvailableSpaces,
                Price_per_hour = (double)p.PricePerHour,
                Description = p.Description
            }).ToList();

            return new SearchResultDto
            {
                Status = "OK",
                Results = results
            };
        }

        public async Task<ParkingLotResponseDto> UpdateParkingLot(string id, CreateParkingLotDto updateDto)
        {
            var parkingLot = await _context.ParkingLots
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (parkingLot == null)
                throw new KeyNotFoundException("Không tìm thấy bãi đỗ xe");

            parkingLot.Name = updateDto.Name;
            parkingLot.Address = updateDto.Formatted_address;
            parkingLot.Latitude = updateDto.Geometry.Location.Lat;
            parkingLot.Longitude = updateDto.Geometry.Location.Lng;
            parkingLot.TotalSpaces = updateDto.TotalSpaces;
            parkingLot.AvailableSpaces = updateDto.AvailableSpaces;
            parkingLot.PricePerHour = updateDto.PricePerHour;
            parkingLot.OpeningTime = updateDto.OpeningTime;
            parkingLot.ClosingTime = updateDto.ClosingTime;
            parkingLot.IsOpen24Hours = updateDto.IsOpen24Hours;
            parkingLot.Description = string.IsNullOrEmpty(updateDto.Description)
                ? $"Bãi đỗ xe tại {updateDto.Compound.District}, {updateDto.Compound.Province}"
                : updateDto.Description;
            parkingLot.ContactNumber = updateDto.ContactNumber;
            parkingLot.UpdatedAt = DateTime.UtcNow;

            // Xử lý upload hình ảnh mới nếu có
            if (updateDto.Images != null && updateDto.Images.Any())
            {
                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "parkinglots");
                Directory.CreateDirectory(uploadPath);

                foreach (var image in updateDto.Images)
                {
                    if (image.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                        var filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        parkingLot.Images.Add(new ParkingLotImage
                        {
                            ImageUrl = $"/uploads/parkinglots/{fileName}",
                            IsMain = parkingLot.Images.Count == 0
                        });
                    }
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

        public async Task<SearchResultDto> SearchNearbyParkingLots(
            double latitude, 
            double longitude, 
            int radius,
            int limit)
        {
            // Đầu tiên, lấy tất cả bãi đỗ xe về bộ nhớ
            var parkingLots = await _context.ParkingLots
                .Include(p => p.Images)
                .AsNoTracking()
                .ToListAsync();

            // Sau đó lọc và sắp xếp trong bộ nhớ
            var nearbyParkingLots = parkingLots
                .Select(p => new 
                { 
                    ParkingLot = p, 
                    Distance = CalculateDistance(latitude, longitude, p.Latitude, p.Longitude) 
                })
                .Where(x => x.Distance <= radius)
                .OrderBy(x => x.Distance)
                .Take(limit)
                .Select(x => x.ParkingLot)
                .ToList();

            if (!nearbyParkingLots.Any())
            {
                return new SearchResultDto 
                { 
                    Status = "ZERO_RESULTS", 
                    Results = new List<ParkingLotResponseDto>(),
                    Metadata = new SearchMetadata
                    {
                        Total = 0,
                        Limit = limit
                    }
                };
            }

            var results = nearbyParkingLots.Select(p => new ParkingLotResponseDto
            {
                Place_id = p.Place_id,
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
                Opening_hours = new OpeningHours
                {
                    Open_now = p.IsOpen24Hours || IsOpenNow(p.OpeningTime, p.ClosingTime),
                    Weekday_text = new[]
                    {
                        $"Giờ mở cửa: {(p.IsOpen24Hours ? "24/7" : $"{p.OpeningTime} - {p.ClosingTime}")}"
                    }
                },
                Photos = p.Images?.Select(img => new Photo
                {
                    Photo_reference = img.ImageUrl
                }).ToList() ?? new List<Photo>(),
                Formatted_phone_number = p.ContactNumber,
                Total_spaces = p.TotalSpaces,
                Available_spaces = p.AvailableSpaces,
                Price_per_hour = (double)p.PricePerHour
            }).ToList();

            return new SearchResultDto
            {
                Status = "OK",
                Results = results,
                Metadata = new SearchMetadata
                {
                    Total = results.Count(),
                    Limit = limit
                }
            };
        }
    }
} 