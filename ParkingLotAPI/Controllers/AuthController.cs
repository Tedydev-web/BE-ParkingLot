using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ParkingLotAPI.Models;
using ParkingLotAPI.Models.Auth;
using ParkingLotAPI.Services;
using ParkingLotAPI.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ParkingLotAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtService _jwtService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new AuthResult 
                    { 
                        Success = false,
                        Errors = ModelState.Values.SelectMany(x => x.Errors.Select(xx => xx.ErrorMessage)).ToList()
                    });

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var token = await _jwtService.GenerateJwtToken(user);
                    return Ok(token);
                }

                return BadRequest(new AuthResult 
                { 
                    Success = false,
                    Errors = result.Errors.Select(x => x.Description).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AuthResult 
                { 
                    Success = false,
                    Errors = new List<string> { "Có lỗi xảy ra khi đăng ký", ex.Message }
                });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new AuthResult 
                    { 
                        Success = false,
                        Errors = ModelState.Values.SelectMany(x => x.Errors.Select(xx => xx.ErrorMessage)).ToList()
                    });

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                    return BadRequest(new AuthResult 
                    { 
                        Success = false,
                        Errors = new List<string> { "Email hoặc mật khẩu không đúng" }
                    });

                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                if (result.Succeeded)
                {
                    var token = await _jwtService.GenerateJwtToken(user);
                    return Ok(token);
                }

                return BadRequest(new AuthResult 
                { 
                    Success = false,
                    Errors = new List<string> { "Email hoặc mật khẩu không đúng" }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AuthResult 
                { 
                    Success = false,
                    Errors = new List<string> { "Có lỗi xảy ra khi đăng nhập", ex.Message }
                });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new AuthResult 
                { 
                    Success = false,
                    Errors = ModelState.Values.SelectMany(x => x.Errors.Select(xx => xx.ErrorMessage)).ToList()
                });

            var result = await _jwtService.VerifyAndGenerateToken(model.Token, model.RefreshToken);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
} 