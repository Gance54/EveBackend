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
                TokenType = "Bearer",
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

        [HttpPost("verify-token")]
        public IActionResult VerifyToken([FromBody] TokenVerificationRequest request)
        {
            var principal = _jwtService.ValidateToken(request.Token);
            if (principal == null)
                return Unauthorized(new { message = "Invalid or expired token." });

            // Get user info from claims
            var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var emailClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { message = "Invalid token claims." });

            return Ok(new
            {
                UserId = userId,
            });
        }

        [HttpPost("get-user")]
        public async Task<IActionResult> GetUser([FromBody] GetUserRequest request)
        {
            var principal = _jwtService.ValidateToken(request.Token);
            if (principal == null)
                return Unauthorized(new { message = "Invalid or expired token." });

            var tokenUserId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (tokenUserId != request.UserId.ToString())
                return Unauthorized(new { message = "Token does not match user." });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(new
            {
                user.Id,
                user.Email,
                user.IsSubscribed,
                user.CreatedAt
                // Exclude PasswordHash
            });
        }
    }
}
