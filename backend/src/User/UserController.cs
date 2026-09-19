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
    public async Task<IActionResult> Register([FromBody] CreateUserDTO request)
    {
        try
        {
            var user = await _userService.CreateUserAsync(request);
            return Ok(new UserCreatedResponseDTO
            {
                Message = "User created successfully",
                UserId = user.Id
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new UserMessageResponseDTO { Message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new UserMessageResponseDTO
            {
                Message = "An unexpected error occurred."
            });
        }
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var profile = await _userService.GetCurrentUserAsync(User);
        if (profile is null)
        {
            return Unauthorized();
        }

        return Ok(profile);
    }
}
