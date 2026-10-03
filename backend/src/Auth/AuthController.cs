using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace PasswordVault.Auth;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthDTO credentials)
    {
        try
        {
            (string tokenString, CookieOptions cookieOptions) = await _authService.GetAuthTokenAsync(credentials);
            Response.Cookies.Append("auth_token", tokenString, cookieOptions);

            return Ok(new AuthMessageResponseDTO
            {
                Message = "Logged in successfully"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new AuthMessageResponseDTO { Message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new AuthMessageResponseDTO
            {
                Message = "An unexpected error occurred."
            });
        }
    }

    [Authorize]
    [HttpPut("new-password")]
    public async Task<IActionResult> ChangePassword([FromBody] NewPasswordDTO request)
    {
        try
        {
            if (!Guid.TryParse(User.Identity?.Name, out var userUuid)) return Unauthorized();

            await _authService.ChangePasswordAsync(userUuid, request.NewPassword);
            return Ok(new AuthMessageResponseDTO
            {
                Message = "Password updated Successfully"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new AuthMessageResponseDTO { Message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new AuthMessageResponseDTO
            {
                Message = "An unexpected error occurred."
            });
        }
    }

    [HttpPost("logout")]
    public IActionResult logout()
    {
        Response.Cookies.Delete("auth_token");
        return Ok(new AuthMessageResponseDTO
        {
            Message = "Logged out successfully"
        });
    }
}
