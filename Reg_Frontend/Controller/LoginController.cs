using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;


[ApiController]
[Route("api/users")]
public class LoginController : ControllerBase
{
    private readonly UserService _userService;

    public LoginController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            bool success = await _userService.SaveUserAsync(
                request.FirstName,
                request.LastName,
                request.Email,
                request.Password,
                request.State,
                request.Organization);

            if (success)
                return Ok(new { success = true, message = "User registered successfully." });
            else
                return BadRequest(new { success = false, message = "Failed to register user." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest login)
    {
        try
        {
            bool isValid = await _userService.ValidateUserAsync(login.Email, login.Password);
            if (isValid)
                return Ok(new { success = true, message = "Login successful." });
            else
                return Unauthorized(new { success = false, message = "Invalid email or password." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}

