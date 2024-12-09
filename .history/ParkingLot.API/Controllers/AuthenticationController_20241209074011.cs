using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ParkingLot.Core.Entities;
using ParkingLot.API.Models;
using System.Transactions;
using Microsoft.AspNetCore.Authorization;

namespace ParkingLot.API.Controllers
{
    /// <summary>
    /// Xử lý các hoạt động xác thực
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Tags("Xác thực")]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ILogger<AuthenticationController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Đăng ký một người dùng mới
        /// </summary>
        /// <param name="model">Chi tiết đăng ký người dùng</param>
        /// <returns>Kết quả của yêu cầu đăng ký</returns>
        /// <response code="201">Người dùng đã đăng ký thành công</response>
        /// <response code="400">Nếu đăng ký thất bại</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            // Inject DbContext vào constructor
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                if (await _userManager.FindByEmailAsync(model.Email) != null)
                    return BadRequest(new { Loi = "Email đã được đăng ký" });

                if (await _userManager.FindByNameAsync(model.Username) != null)
                    return BadRequest(new { Loi = "Tên người dùng đã được sử dụng" });

                var user = new ApplicationUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                    return BadRequest(new { Loi = result.Errors.Select(e => e.Description) });

                // Gán vai trò mặc định nếu không có chỉ định
                var role = !string.IsNullOrEmpty(model.Role) ? model.Role : "user";
                var normalizedRole = role.ToUpperInvariant();

                if (!await _roleManager.RoleExistsAsync(normalizedRole))
                {
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole(normalizedRole));
                    if (!roleResult.Succeeded)
                        return BadRequest(new { Loi = "Tạo vai trò thất bại" });
                }

                await _userManager.AddToRoleAsync(user, normalizedRole);

                scope.Complete();
                
                // Tạo token để xác nhận email
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                
                return CreatedAtAction(nameof(Register), new 
                { 
                    id = user.Id,
                    thongBao = "Đăng ký thành công. Vui lòng xác nhận email của bạn."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong quá trình đăng ký người dùng");
                return BadRequest(new { Loi = "Đăng ký không thành công" });
            }
        }

        /// <summary>
        /// Xác thực người dùng và tạo token JWT
        /// </summary>
        /// <param name="model">Thông tin đăng nhập</param>
        /// <returns>Token JWT và thông tin người dùng</returns>
        /// <response code="200">Xác thực thành công</response>
        /// <response code="401">Thông tin không hợp lệ</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                var userName = model.Username ?? throw new ArgumentNullException(nameof(model.Username));
                if (string.IsNullOrEmpty(userName))
                {
                    return BadRequest("Username is required.");
                }

                var user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                    return Unauthorized(new { Loi = "Thông tin không hợp lệ" });

                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, true);
                if (!result.Succeeded)
                {
                    if (result.IsLockedOut)
                        return BadRequest(new { Loi = "Tài khoản bị khóa. Vui lòng thử lại sau." });
                    
                    if (result.IsNotAllowed)
                        return BadRequest(new { Loi = "Tài khoản không được phép đăng nhập" });
                    
                    return Unauthorized(new { Loi = "Thông tin đăng nhập không hợp lệ" });
                }

                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? string.Empty;

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user?.Id ?? throw new InvalidOperationException("User ID is null")),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                    new Claim(ClaimTypes.Role, role)
                };

                var token = await GenerateJwtTokenAsync(user, roles);

                _logger.LogInformation("Người dùng {UserName} đăng nhập thành công", user.UserName);

                return Ok(new
                {
                    Token = token,
                    HetHan = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:DurationInMinutes"])),
                    NguoiDung = new { user.Id, user.UserName, user.Email },
                    VaiTro = roles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong quá trình đăng nhập");
                return BadRequest(new { Loi = "Đăng nhập không thành công" });
            }
        }

        /// <summary>
        /// Làm mới token xác thực
        /// </summary>
        /// <returns>JWT token mới</returns>
        /// <response code="200">Token được làm mới thành công</response>
        /// <response code="401">Token không hợp lệ hoặc hết hạn</response>
        [HttpPost("refresh-token")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null)
                return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);
            var token = await GenerateJwtTokenAsync(user, roles);

            return Ok(new
            {
                Token = token,
                HetHan = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:DurationInMinutes"]))
            });
        }

        private async Task<string> GenerateJwtTokenAsync(ApplicationUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            // Thêm role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.ToLower()));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"] ?? string.Empty));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:DurationInMinutes"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
