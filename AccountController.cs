using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace EveAuthApi
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;

        public AccountController(ApplicationDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Email already registered.");

            var user = new User
            {
                Email = request.Email,
                PasswordHash = request.Password, // Password already hashed from frontend
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Account created successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            // Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return BadRequest("Invalid email or password.");

            // Verify password (assuming frontend sends SHA256 hash)
            if (user.PasswordHash != request.Password)
                return BadRequest("Invalid email or password.");

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var issuedAt = DateTime.UtcNow;

            var response = new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 3600, // 1 hour in seconds
                TokenType = "Bearer",
                IssuedAt = issuedAt,
                User = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email,
                    IsSubscribed = user.IsSubscribed,
                    CreatedAt = user.CreatedAt
                }
            };

            return Ok(response);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new { u.Id, u.Email, u.IsSubscribed, u.CreatedAt })
                .ToListAsync();
            
            return Ok(users);
        }
    }

    public class LoginResponse
    {
        public string AccessToken { get; set; } = "";
        public string RefreshToken { get; set; } = "";
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; } = "";
        public DateTime IssuedAt { get; set; }
        public UserInfo User { get; set; } = new();
    }

    public class UserInfo
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public bool IsSubscribed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
