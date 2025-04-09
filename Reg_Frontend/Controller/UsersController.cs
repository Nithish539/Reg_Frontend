using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    // Validate Email
    [HttpPost("validate-email")]
    public async Task<IActionResult> ValidateEmail([FromBody] EmailRequest request)
    {
        bool exists = await _userService.ValidateEmailAsync(request.Email);
        if (exists)
            return Ok(new { valid = true, message = "Email is valid." });
        else
            return BadRequest(new { valid = false, message = "Email not found." });
    }

    // Reset Password
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        bool updated = await _userService.ResetPasswordAsync(request.Email, request.NewPassword);
        if (updated)
            return Ok(new { success = true, message = "Password reset successful." });
        else
            return BadRequest(new { success = false, message = "Failed to reset password." });
    }

    // Forgot Password (Check if user exists before showing reset link in frontend toast)
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] EmailRequest request)
    {
        bool exists = await _userService.ValidateEmailAsync(request.Email);
        if (!exists)
            return NotFound(new { success = false, message = "Email not found." });

        // You can also log the attempt here or trigger an actual email in future
        return Ok(new { success = true, message = "Reset link generated." });
    }

    // ✅ Internal DTOs
    public class EmailRequest
    {
        public string Email { get; set; }
    }
    public class ResetPasswordRequest
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
    }
}
