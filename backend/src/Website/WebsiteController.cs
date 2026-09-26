using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PasswordVault.Website;

[Authorize]
[ApiController]
[Route("[controller]")]
public class WebsiteController : ControllerBase
{
    private readonly WebsiteService _websiteService;

    public WebsiteController(WebsiteService websiteService)
    {
        _websiteService = websiteService;
    }

    [HttpPost()]
    public async Task<ActionResult<NewWebsiteResponse>> CreateNewWebsite([FromBody] NewWebsiteDTO newUser)
    {
        try
        {
            var userIdValue = User.Identity?.Name;

            if (!Guid.TryParse(userIdValue, out var userUuid)) return Unauthorized();

            NewWebsiteResponse response = await _websiteService.CreateNewWebsiteAsync(newUser, userUuid);
            return Ok(response);
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
}
