using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PasswordVault.Websites;

[ApiController]
[Authorize]
[Route("website")]
public class WebsiteController : ControllerBase
{
    private readonly WebsiteService _websiteService;

    public WebsiteController(WebsiteService websiteService)
    {
        _websiteService = websiteService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WebsiteListResponse>>> GetWebsites([FromQuery] Guid websiteUuid)
    {
        try
        {
            var userUuidValue = User.Identity?.Name;
            if (!Guid.TryParse(userUuidValue, out var userUuid))
            {
                return Unauthorized();
            }

            var websites = await _websiteService.GetWebsitesAsync(userUuid, websiteUuid);
            return Ok(websites);
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

    [HttpPut("{websiteUuid:guid}")]
    public async Task<ActionResult<WebsiteUpdateResponse>> UpdateWebsite(
        [FromRoute] Guid websiteUuid,
        [FromBody] UpdateWebsiteRequest request)
    {
        try
        {
            var userUuidValue = User.Identity?.Name;
            if (!Guid.TryParse(userUuidValue, out var userUuid))
            {
                return Unauthorized();
            }

            var response = await _websiteService.UpdateWebsiteAsync(userUuid, websiteUuid, request);
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
