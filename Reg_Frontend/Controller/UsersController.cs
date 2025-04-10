using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using BCrypt.Net;
using Reg_Frontend.Models;

namespace Reg_Frontend.Models.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserService _userService;

        public UsersController(IConfiguration configuration)
        {
            _configuration = configuration;
            _userService = new UserService(configuration);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto user)
        {
            var success = await _userService.SaveUserAsync(user);
            if (!success)
            {
                return BadRequest("Registration failed");
            }

            return Ok(new { message = "Registration successful" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            var user = await _userService.ValidateUserAsync(login.Email, login.Password);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            return Ok(new { message = "Login successful" });
        }


        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest("Email and new password are required.");
            }

            var normalizedEmail = request.Email.Trim().ToLower();

            bool emailExists = await _userService.ValidateEmailAsync(normalizedEmail);
            if (!emailExists)
            {
                return BadRequest("Invalid email");
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            bool resetSuccess = await _userService.ResetPasswordAsync(normalizedEmail, hashedPassword);

            if (resetSuccess)
            {
                return Ok("Password has been reset successfully");
            }

            return StatusCode(500, "Something went wrong while resetting the password.");
        }

        [HttpGet("validateemail")]
        public async Task<IActionResult> ValidateEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest("Email is required");
            }

            var normalizedEmail = email.Trim().ToLower();
            bool exists = await _userService.ValidateEmailAsync(normalizedEmail);

            return exists ? Ok() : NotFound();
        }
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
    }
}
