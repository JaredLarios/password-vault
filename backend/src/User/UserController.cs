using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PasswordVault.Users;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<NewUserResponse>> Register([FromBody] NewUserDTO newUser)
    {
        try
        {
            NewUserResponse user = await _userService.CreateUserAsync(newUser);
            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserProfileResponse>> GetProfile()
    {
        try
        {
            var userIdValue = User.Identity?.Name;

            if (!Guid.TryParse(userIdValue, out var userUuid))  return Unauthorized();
            
            UserProfileResponse profile = await _userService.GetCurrentUserAsync(userUuid);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp);
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }
}
