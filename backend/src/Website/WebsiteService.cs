using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PasswordVault.Common.Database;
using PasswordVault.Common.Interfaces;
using PasswordVault.Common.Models;

namespace PasswordVault.Websites;

public class WebsiteService
{
    private readonly AppDbContext _context;
    private readonly ICrypto _crypto;
    private readonly IHash _sha256;

    public WebsiteService(
        AppDbContext context,
        [FromKeyedServices("services")] ICrypto crypto,
        [FromKeyedServices("sha256")] IHash sha256)
    {
        _context = context;
        _crypto = crypto;
        _sha256 = sha256;
    }

    public async Task<List<WebsiteListResponse>> GetWebsitesAsync(Guid userUuid, Guid websiteUuid)
    {
        var websites = await GetWebsitesForUserAsync(userUuid, websiteUuid);

        return websites.Select(w => new WebsiteListResponse
        {
            WebsiteName = w.WebsiteName,
            Urls = w.Links
                .Where(link => link.isActive)
                .Select(link => link.Url)
                .ToList(),
            Credentials = w.Credentials
                .Where(credential => credential.isActive)
                .Select(credential => new WebsiteListCredentialResponse
                {
                    WebsiteUserId = Guid.Parse(credential.Uuid),
                    WebsiteUsername = string.IsNullOrEmpty(credential.UsernameFer)
                        ? string.Empty
                        : _crypto.GetDecryptedText(credential.UsernameFer),
                    WebistePassword = string.IsNullOrEmpty(credential.PasswordFer)
                        ? string.Empty
                        : _crypto.GetDecryptedText(credential.PasswordFer)
                })
                .ToList()
        }).ToList();
    }

    private async Task<List<WebsiteModel>> GetWebsitesForUserAsync(Guid userUuid, Guid websiteUuid)
    {
        var query = _context.Websites
            .Include(website => website.Links)
            .Include(website => website.Credentials)
            .Where(website => website.User.Uuid == userUuid && website.isActive);

        if (websiteUuid != Guid.Empty)
        {
            query = query.Where(website => website.Uuid == websiteUuid);
        }

        return await query.ToListAsync();
    }

    public async Task<WebsiteUpdateResponse> UpdateWebsiteAsync(Guid userUuid, Guid websiteUuid, UpdateWebsiteDto request)
    {
        var website = await _context.Websites
            .Include(w => w.Links)
            .Include(w => w.Credentials)
            .FirstOrDefaultAsync(w => w.Uuid == websiteUuid && w.User.Uuid == userUuid && w.isActive);

        if (website == null)
        {
            throw new ArgumentException("Website not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.WebsiteName))
        {
            website.WebsiteName = request.WebsiteName;
        }

        if (!string.IsNullOrWhiteSpace(request.WebsiteUrl))
        {
            var existingLink = website.Links.FirstOrDefault(link => link.isActive);

            if (existingLink == null)
            {
                website.Links.Add(new UserWebsiteLinkModel
                {
                    Url = request.WebsiteUrl,
                    WebsiteId = website.Id,
                    isActive = true
                });
            }
            else
            {
                existingLink.Url = request.WebsiteUrl;
            }
        }

        if (!string.IsNullOrWhiteSpace(request.WebsiteUsername) || !string.IsNullOrWhiteSpace(request.WebistePassword))
        {
            var credential = website.Credentials.FirstOrDefault(c => c.isActive);
            if (credential == null)
            {
                credential = new UserWebsiteCredentialModel
                {
                    WebsiteId = website.Id,
                    isActive = true
                };
                website.Credentials.Add(credential);
            }

            if (!string.IsNullOrWhiteSpace(request.WebsiteUsername))
            {
                credential.UsernameFer = _crypto.GetEncryptedText(request.WebsiteUsername);
                credential.UsernameSha = _sha256.GetHash(request.WebsiteUsername);
            }

            if (!string.IsNullOrWhiteSpace(request.WebistePassword))
            {
                credential.PasswordFer = _crypto.GetEncryptedText(request.WebistePassword);
                credential.PasswordSha = _sha256.GetHash(request.WebistePassword);
            }
        }

        website.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new WebsiteUpdateResponse
        {
            Message = "Website credentials updated successfully."
        };
    }
}
