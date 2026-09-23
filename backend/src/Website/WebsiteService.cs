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

    public WebsiteService(AppDbContext context, [FromKeyedServices("services")] ICrypto crypto)
    {
        _context = context;
        _crypto = crypto;
    }

    public async Task<List<WebsiteListResponse>> GetWebsitesAsync(Guid userUuid, Guid websiteUuid)
    {
        var user = await _context.Users
            .Include(u => u.Websites)
            .ThenInclude(w => w.Urls)
            .Include(u => u.Websites)
            .ThenInclude(w => w.Credentials)
            .FirstOrDefaultAsync(u => u.Uuid == userUuid && u.isActive);

        if (user == null)
        {
            throw new ArgumentException("User not found.");
        }

        var websites = user.Websites
            .Where(w => w.isActive && (websiteUuid == Guid.Empty || w.Uuid == websiteUuid))
            .ToList();

        return websites.Select(w => new WebsiteListResponse
        {
            WebsiteName = w.WebsiteName,
            Urls = w.Urls
                .Where(url => url.isActive)
                .Select(url => url.Url)
                .ToList(),
            Credentials = w.Credentials
                .Where(c => c.isActive)
                .Select(c => new WebsiteListCredentialResponse
                {
                    WebsiteUserId = c.WebsiteUserId,
                    WebsiteUsername = _crypto.GetDecryptedText(c.WebsiteUsername),
                    WebistePassword = _crypto.GetDecryptedText(c.WebsitePassword)
                })
                .ToList()
        }).ToList();
    }

    public async Task<WebsiteUpdateResponse> UpdateWebsiteAsync(Guid userUuid, Guid websiteUuid, UpdateWebsiteRequest request)
    {
        if (request == null)
        {
            throw new ArgumentException("Invalid request.");
        }

        var website = await _context.Websites
            .Include(w => w.Urls)
            .Include(w => w.Credentials)
            .FirstOrDefaultAsync(w => w.Uuid == websiteUuid && w.UserId == _context.Users
                .Where(u => u.Uuid == userUuid && u.isActive)
                .Select(u => u.Id)
                .FirstOrDefault() && w.isActive);

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
            var existingUrl = website.Urls.FirstOrDefault(url => url.isActive);

            if (existingUrl == null)
            {
                website.Urls.Add(new WebsiteUrlModel
                {
                    Url = request.WebsiteUrl,
                    WebsiteId = website.Id,
                    isActive = true
                });
            }
            else
            {
                existingUrl.Url = request.WebsiteUrl;
            }
        }

        if (!string.IsNullOrWhiteSpace(request.WebsiteUsername) || !string.IsNullOrWhiteSpace(request.WebistePassword))
        {
            var credential = website.Credentials.FirstOrDefault(c => c.isActive) ?? new WebsiteCredentialModel
            {
                WebsiteUserId = Guid.NewGuid(),
                WebsiteId = website.Id,
                isActive = true
            };

            if (!string.IsNullOrWhiteSpace(request.WebsiteUsername))
            {
                credential.WebsiteUsername = _crypto.GetEncryptedText(request.WebsiteUsername);
            }

            if (!string.IsNullOrWhiteSpace(request.WebistePassword))
            {
                credential.WebsitePassword = _crypto.GetEncryptedText(request.WebistePassword);
            }

            if (website.Credentials.Any(c => c.Id == credential.Id) == false)
            {
                website.Credentials.Add(credential);
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
