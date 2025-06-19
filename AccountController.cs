using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EveAuthApi
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
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
