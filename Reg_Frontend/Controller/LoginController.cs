using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        try
        {
            _userService.SaveUser(request.FirstName, request.LastName, request.Email,
                                  request.Password, request.State, request.Organization);

            return Ok(new { success = true, message = "User registered successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest login)
    {
        try
        {
            bool isValid = _userService.ValidateUser(login.Email, login.Password);
            if (isValid)
            {
                return Ok(new { success = true, message = "Login successful." });
            }
            else
            {
                return Unauthorized(new { success = false, message = "Invalid email or password." });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}
