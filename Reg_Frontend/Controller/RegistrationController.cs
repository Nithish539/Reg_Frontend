using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/registration")]
public class RegistrationController : ControllerBase
{
    private readonly UserService _userService;

    public RegistrationController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Map to UserDto
            var userDto = new UserDto
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Password = request.Password,
                State = request.State,
                Organization = request.Organization
            };

            bool success = await _userService.SaveUserAsync(userDto);

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
}

public class RegisterRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string State { get; set; }
    public string Organization { get; set; }
}
