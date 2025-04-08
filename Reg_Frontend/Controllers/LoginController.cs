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
    public IActionResult Register([FromBody] UserModel user)
    {
        _userService.SaveUser(user.FirstName, user.LastName, user.Email, user.Password, user.State, user.Organization);
        return Ok(new { success = true, message = "User registered successfully." });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel login)
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
}
