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

    [HttpPost]
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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WebsiteListResponse>>> GetWebsites([FromQuery] Guid? websiteUuid)
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

    [HttpGet("Password")]
    public async Task<ActionResult<int>> TestPasswordSec([FromQuery] string passwordHash)
    {
        try
        {
            var websites = await _websiteService.GetPasswordSecurity(passwordHash);
            return Ok(websites);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPut("{websiteUuid:guid}")]
    public async Task<ActionResult<NewWebsiteResponse>> UpdateWebsite(
        [FromRoute] Guid websiteUuid,
        [FromBody] UpdateWebsiteDTO request)
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

    [HttpDelete("{credentialUuid:guid}")]
    public async Task<ActionResult<NewWebsiteResponse>> DeleteWebsiteCredential(
        [FromRoute] Guid credentialUuid
    )
    {
        try
        {
            var userUuidValue = User.Identity?.Name;
            if (!Guid.TryParse(userUuidValue, out var userUuid))
            {
                return Unauthorized();
            }

            var response = await _websiteService.DeleteWebsiteCredentialAsync(userUuid, credentialUuid);
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
