using AI.CustomerSupport.API.Data;
using AI.CustomerSupport.API.DTOs.Auth;
using AI.CustomerSupport.API.Models;
using AI.CustomerSupport.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AI.CustomerSupport.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;

        public AuthController(AppDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
    RegisterRequest request)
        {
            if (await _context.Users
                .AnyAsync(x => x.Email == request.Email))
            {
                return BadRequest("Email already exists");
            }

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        request.Password),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return Ok();
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(
    LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x =>
                    x.Email == request.Email);

            if (user == null)
            {
                return Unauthorized();
            }

            var valid =
                BCrypt.Net.BCrypt.Verify(
                    request.Password,
                    user.PasswordHash);

            if (!valid)
            {
                return Unauthorized();
            }

            var token =
                _jwtService.GenerateToken(user);

            return Ok(new LoginResponse
            {
                Token = token
            });
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(new
            {
                User.Identity?.Name
            });
        }
    }
}
