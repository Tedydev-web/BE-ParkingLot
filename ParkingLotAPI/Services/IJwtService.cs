using ParkingLotAPI.Models;
using ParkingLotAPI.Models.Auth;

namespace ParkingLotAPI.Services
{
    public interface IJwtService
    {
        Task<AuthResult> GenerateJwtToken(ApplicationUser user);
        Task<AuthResult> VerifyAndGenerateToken(string token, string refreshToken);
    }
} 