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

            var issuedAt = DateTime.UtcNow;
            var accessTokenExpiresAt = issuedAt.AddHours(1);
            var refreshTokenExpiresAt = issuedAt.AddDays(30);

            // Revoke all existing tokens for this user
            var existingAccessTokens = await _context.AccessTokens
                .Where(at => at.UserId == user.Id && !at.IsRevoked)
                .ToListAsync();
            
            var existingRefreshTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
                .ToListAsync();

            foreach (var token in existingAccessTokens)
            {
                token.IsRevoked = true;
            }
            
            foreach (var token in existingRefreshTokens)
            {
                token.IsRevoked = true;
            }

            // Generate new access token
            var accessToken = _jwtService.GenerateAccessToken(user);
            
            // Save new access token to database
            var newAccessToken = new AccessToken
            {
                Token = accessToken,
                UserId = user.Id,
                ExpiresAt = accessTokenExpiresAt,
                CreatedAt = issuedAt
            };
            
            _context.AccessTokens.Add(newAccessToken);

            // Generate new refresh token
            var refreshToken = _jwtService.GenerateRefreshToken();
            
            // Save new refresh token to database
            var newRefreshToken = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = refreshTokenExpiresAt,
                CreatedAt = issuedAt
            };
            
            _context.RefreshTokens.Add(newRefreshToken);

            // Save changes to database
            await _context.SaveChangesAsync();

            var response = new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresIn = 3600, // 1 hour in seconds
                RefreshTokenExpiresIn = 2592000, // 30 days in seconds (1 month)
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
}
