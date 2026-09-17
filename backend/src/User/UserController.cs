using System.Security.Claims;
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
    public async Task<IActionResult> Register([FromBody] CreateUserRequest request)
    {
        try
        {
            var user = await _userService.CreateUserAsync(request);
            return Ok(new
            {
                message = "User created successfully",
                userId = user.Id
            });
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
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var profile = await _userService.GetCurrentUserAsync(userId.Value);
        if (profile is null)
        {
            return NotFound(new { message = "User not found." });
        }

        return Ok(profile);
    }

    private int? GetCurrentUserId()
    {
        if (HttpContext.Items.TryGetValue("UserId", out var item) && item is int userId)
        {
            return userId;
        }

        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("userId");

        if (int.TryParse(idClaim, out var parsedId))
        {
            return parsedId;
        }

        return null;
    }
}
