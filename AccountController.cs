using Microsoft.AspNetCore.Mvc;

namespace EveAuthApi
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private static List<User> _users = new(); // Use DB in real version

        [HttpPost("register")]
        public IActionResult Register(RegisterRequest request)
        {
            if (_users.Any(u => u.Email == request.Email))
                return BadRequest("Email already registered.");

            var user = new User
            {
                Id = _users.Count + 1,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _users.Add(user);

            return Ok(new { message = "Account created successfully." });
        }

        // test REST comm
        [HttpGet("sayhi")]
        public ActionResult<string> SayHi()
        {
            return Ok("Hello from backend!");
        }
    }
}
