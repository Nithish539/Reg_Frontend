using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
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
            return success ? Ok(new { message = "Registration successful" }) : BadRequest("Registration failed");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userService.GetUserByEmailAsync(request.Email.Trim().ToLower());
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return BadRequest("Invalid email or password.");
            }

            return Ok("Login successful");
        }

        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequest("Email and new password are required.");

            var normalizedEmail = request.Email.Trim().ToLower();
            if (!await _userService.ValidateEmailAsync(normalizedEmail))
                return BadRequest("Invalid email");

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            bool resetSuccess = await _userService.ResetPasswordAsync(normalizedEmail, hashedPassword);

            return resetSuccess ? Ok("Password has been reset successfully") : StatusCode(500, "Something went wrong while resetting the password.");
        }

        [HttpGet("validateemail")]
        public async Task<IActionResult> ValidateEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return BadRequest("Email is required");

            var normalizedEmail = email.Trim().ToLower();
            bool exists = await _userService.ValidateEmailAsync(normalizedEmail);

            return exists ? Ok() : NotFound();
        }

        [HttpPost("generate-reset-token")]
        public async Task<IActionResult> GenerateResetToken([FromBody] EmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Email is required.");

            var token = await _userService.GenerateAndStoreResetTokenAsync(request.Email.Trim().ToLower());
            return token != null
                ? Ok(new { message = "Token generated successfully", token })
                : BadRequest("Failed to generate reset token.");
        }

        [HttpPost("validate-reset-token")]
        public async Task<IActionResult> ValidateResetToken([FromBody] ResetTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Token))
                return BadRequest("Email and token are required.");

            bool isValid = await _userService.IsResetTokenValidAsync(request.Email.Trim().ToLower(), request.Token);
            return isValid ? Ok(new { message = "Token is valid." }) : BadRequest("Invalid or expired token.");
        }

        [HttpPost("reset-password-token")]
        public async Task<IActionResult> ResetPasswordWithToken([FromBody] ResetPasswordWithTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Token) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest("All fields are required.");
            }

            var normalizedEmail = request.Email.Trim().ToLower();
            bool isValid = await _userService.IsResetTokenValidAsync(normalizedEmail, request.Token);

            if (!isValid) return BadRequest("Invalid or expired token.");

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            bool resetSuccess = await _userService.ResetPasswordAsync(normalizedEmail, hashedPassword);

            return resetSuccess ? Ok("Password has been reset successfully.") : StatusCode(500, "Something went wrong while resetting the password.");
        }

        [HttpPost("verifyemail")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Email is required.");

            string normalizedEmail = request.Email.Trim().ToLower();

            var user = await _userService.GetUserByEmailAsync(normalizedEmail);
            if (user == null) return NotFound("Invalid email.");

            string token = await _userService.GenerateAndStoreResetTokenAsync(normalizedEmail);
            if (token == null) return StatusCode(500, "Failed to generate reset token.");

            return Ok(new { token });
        }
    }

    public class VerifyEmailRequest
    {
        public string Email { get; set; }
    }

    public class ResetPasswordWithTokenRequest
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }

    public class ResetTokenRequest
    {
        public string Email { get; set; }
        public string Token { get; set; }
    }
}
