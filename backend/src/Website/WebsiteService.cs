using System.ComponentModel.DataAnnotations;
using System.Runtime.Intrinsics.Arm;
using Microsoft.EntityFrameworkCore;
using PasswordVault.Common.Database;
using PasswordVault.Common.Interfaces;
using PasswordVault.Common.Models;

namespace PasswordVault.Website;

public class WebsiteService
{
    private readonly AppDbContext _context;
    private readonly ICrypto _crypto;
    private readonly IHash _sha256;
    private readonly IHash _sha1;

    public WebsiteService(
        AppDbContext context,
        [FromKeyedServices("services")] ICrypto crypto,
        [FromKeyedServices("sha256")] IHash sha256,
        [FromKeyedServices("sha1")] IHash sha1
    )
    {
        _context = context;
        _crypto = crypto;
        _sha256 = sha256;
        _sha1 = sha1;
    }

    private async Task<WebsiteModel?> GetWebsiteByNameOrLinkAsync(string websiteName, string websiteLink, Guid userUuid)
    {
        return await _context
            .Websites
            .Where(
                website => (website.WebsiteName == websiteName ||
                website.WebsiteLinks.Any(
                    link => link.WebsiteUrl == websiteLink)) &&
                website.User.Uuid == userUuid &&
                website.isActive)
            .FirstOrDefaultAsync();
    }

    private async Task<UserModel?> GetUserByUuidAsync(Guid userUuid)
    {
        return await _context
            .Users
            .Where(user => user.Uuid == userUuid && user.isActive)
            .FirstOrDefaultAsync();
    }

    private WebsiteLinkModel CreateWebsiteLink(string websiteUrl)
    {
        WebsiteLinkModel websiteLink = new WebsiteLinkModel
        {
            Uuid = Guid.NewGuid(),
            WebsiteUrl = websiteUrl
        };

        return websiteLink;
    }

    private WebsiteModel CreateWebsite(UserModel user, NewWebsiteDTO newWebsite)
    {
        return new WebsiteModel
        {
            Uuid = Guid.NewGuid(),
            WebsiteName = newWebsite.WebsiteName,
            User = user,
            WebsiteLinks = new List<WebsiteLinkModel> { CreateWebsiteLink(newWebsite.WebsiteUrl) }
        };
    }

    private WebsiteCredentialModel CreateNewCredential(NewWebsiteDTO newWebsite)
    {
        return new WebsiteCredentialModel
        {
            Uuid = Guid.NewGuid(),
            WebsiteUsernameFer = _crypto.GetEncryptedText(newWebsite.WebsiteUsername),
            WebsiteUsernameSha = _sha256.GetHash(newWebsite.WebsiteUsername),
            WebsitePasswordFer = _crypto.GetEncryptedText(newWebsite.WebsitePassword),
            WebsitePasswordSha = _sha1.GetHash(newWebsite.WebsitePassword)
        };
    }

    public async Task<NewWebsiteResponse> CreateNewWebsiteAsync(NewWebsiteDTO newWebsite, Guid userUuid)
    {
        WebsiteModel? existingWebsite = await GetWebsiteByNameOrLinkAsync(
                                                newWebsite.WebsiteName, newWebsite.WebsiteUrl, userUuid);

        UserModel? user = await GetUserByUuidAsync(userUuid);
        if (user == null) throw new KeyNotFoundException("User not found");

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            WebsiteModel website;

            if (existingWebsite == null)
            {
                website = CreateWebsite(user, newWebsite);
                _context.Websites.Add(website);
            }
            else
            {
                website = existingWebsite;
            }

            website.WebsiteCredentials = new List<WebsiteCredentialModel> { CreateNewCredential(newWebsite) };

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new NewWebsiteResponse { Message = "Website credentials saved successfully." };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<WebsiteModel?> GetWebsiteByNameOrLinkAsync(string websiteName, string websiteLink, Guid userUuid)
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
