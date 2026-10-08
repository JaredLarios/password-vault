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
    private readonly IHttpClientFactory _client;

    public WebsiteService(
        AppDbContext context,
        IHttpClientFactory client,
        [FromKeyedServices("services")] ICrypto crypto,
        [FromKeyedServices("sha256")] IHash sha256,
        [FromKeyedServices("sha1")] IHash sha1
    )
    {
        _context = context;
        _client = client;
        _crypto = crypto;
        _sha256 = sha256;
        _sha1 = sha1;
    }

    private async Task<UserModel?> GetUserByUuidAsync(Guid userUuid)
    {
        return await _context
            .Users
            .Where(user => user.Uuid == userUuid && user.isActive)
            .FirstOrDefaultAsync();
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

    private async Task<WebsiteModel?> GetWebsiteByUuidAsync(Guid userUuid, Guid websiteUuid)
    {
        return await _context.Websites
            .Include(website => website.WebsiteLinks)
            .Include(website => website.WebsiteCredentials)
            .FirstOrDefaultAsync(website =>
                website.Uuid == websiteUuid &&
                website.User.Uuid == userUuid &&
                website.isActive);
    }

    private async Task<List<WebsiteModel>> GetWebsitesByUserUuidAsync(Guid userUuid, Guid? websiteUuid)
    {
        var query = _context.Websites
            .Include(website => website.WebsiteLinks)
            .Include(website => website.WebsiteCredentials)
            .Where(website => website.User.Uuid == userUuid && website.isActive);

        if (websiteUuid.HasValue)
        {
            query = query.Where(website => website.Uuid == websiteUuid);
        }

        return await query.ToListAsync();
    }

    private async Task<List<WebsiteModel>> GetWebsitesByNameAsync(Guid userUuid, string websiteName)
    {
        return await _context
            .Websites
            .Include(website => website.WebsiteLinks)
            .Include(website => website.WebsiteCredentials)
            .Where(website =>
                    website.User.Uuid == userUuid &&
                    EF.Functions.ILike(website.WebsiteName, $"%{websiteName}%") &&
                    website.isActive
            )
            .ToListAsync();
    }

    private async Task<WebsiteCredentialModel?> GetWebsiteCredentialByUuid(Guid userUuid, Guid websiteCredentialUuid)
    {
        return await _context
            .WebsiteCredentials
            .Where(credential =>
                credential.Uuid == websiteCredentialUuid &&
                credential.Website.User.Uuid == userUuid
            )
            .FirstOrDefaultAsync();
    }

    private async Task<int> GetPasswordSecurity(string passwordHash)
    {
        string firstFiveChars = passwordHash[..5];
        string remainingChars = passwordHash[5..].ToLower();

        var clientService = _client.CreateClient("PasswordLeakClient");
        string response = await clientService.GetStringAsync($"/range/{firstFiveChars}");

        string? matchingLine = response
            .ToLower()
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .FirstOrDefault(line => line.StartsWith($"{remainingChars}:"));

        if (matchingLine == null) return 0;

        string passwordCount = matchingLine.Split(":")[1];

        return int.Parse(passwordCount);
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

    private WebsiteCredentialModel CreateNewCredential(NewWebsiteDTO newWebsite, int? count)
    {
        return new WebsiteCredentialModel
        {
            Uuid = Guid.NewGuid(),
            WebsiteUsernameFer = _crypto.GetEncryptedText(newWebsite.WebsiteUsername),
            WebsiteUsernameSha = _sha256.GetHash(newWebsite.WebsiteUsername),
            WebsitePasswordFer = _crypto.GetEncryptedText(newWebsite.WebsitePassword),
            WebsitePasswordSha = _sha1.GetHash(newWebsite.WebsitePassword),
            WebsitePasswordLeakedCount = count
        };
    }


    public async Task<NewWebsiteResponse> CreateNewWebsiteAsync(NewWebsiteDTO newWebsite, Guid userUuid)
    {
        WebsiteModel? existingWebsite = await GetWebsiteByNameOrLinkAsync(
                                                newWebsite.WebsiteName, newWebsite.WebsiteUrl, userUuid);

        UserModel? user = await GetUserByUuidAsync(userUuid);
        if (user == null) throw new KeyNotFoundException("User not found");

        string passwordHash = _sha1.GetHash(newWebsite.WebsitePassword);
        int? passwordLeakedCount = await GetPasswordSecurity(passwordHash);

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

            website.WebsiteCredentials = new List<WebsiteCredentialModel> {
                CreateNewCredential(newWebsite, passwordLeakedCount)
            };

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

    public async Task<List<WebsiteListResponse>> GetWebsitesAsync(Guid userUuid, Guid? websiteUuid, string? websiteName)
    {
        List<WebsiteModel> websites;

        if (websiteUuid != null && !string.IsNullOrEmpty(websiteName)) throw new ArgumentException("Provide either 'websiteUuid' or 'websiteName', but not both.");

        if (string.IsNullOrEmpty(websiteName)) websites = await GetWebsitesByUserUuidAsync(userUuid, websiteUuid);
        else websites = await GetWebsitesByNameAsync(userUuid, websiteName);
        ;

        return websites.Select(website => new WebsiteListResponse
        {
            WebsiteId = website.Uuid,
            WebsiteName = website.WebsiteName,
            Urls = website.WebsiteLinks
                .Where(link => link.isActive)
                .Select(link => link.WebsiteUrl)
                .ToList(),
            Credentials = website.WebsiteCredentials
                .Where(credential => credential.isActive)
                .Select(credential => new WebsiteListCredentialResponse
                {
                    WebsiteUserId = credential.Uuid,
                    WebsiteUsername = _crypto.GetDecryptedText(credential.WebsiteUsernameFer),
                    WebsitePassword = _crypto.GetDecryptedText(credential.WebsitePasswordFer),
                    WebsitePwdLakedCount = credential.WebsitePasswordLeakedCount
                })
                .ToList()
        }).ToList();
    }

    public async Task<NewWebsiteResponse> UpdateWebsiteAsync(Guid userUuid, Guid websiteUuid, UpdateWebsiteDTO request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string passwordHash = string.Empty;
        int? passwordLeakedCount = null;

        var website = await GetWebsiteByUuidAsync(userUuid, websiteUuid);

        if (!string.IsNullOrWhiteSpace(request.WebsitePassword))
        {
            passwordHash = _sha1.GetHash(request.WebsitePassword);
            passwordLeakedCount = await GetPasswordSecurity(passwordHash);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {

            if (website == null) throw new ArgumentException("Website not found.");

            if (!string.IsNullOrWhiteSpace(request.WebsiteName)) website.WebsiteName = request.WebsiteName;


            if (!string.IsNullOrWhiteSpace(request.WebsiteUrl))
            {
                var existingLink = website.WebsiteLinks.FirstOrDefault(link =>
                    link.isActive && link.WebsiteUrl == request.WebsiteUrl);

                if (existingLink == null)
                {
                    website.WebsiteLinks.Add(new WebsiteLinkModel
                    {
                        WebsiteUrl = request.WebsiteUrl,
                        WebsiteId = website.Id
                    });
                }
                else
                {
                    existingLink.WebsiteUrl = request.WebsiteUrl;
                }
            }

            if (!string.IsNullOrWhiteSpace(request.WebsiteUsername) ||
                !string.IsNullOrWhiteSpace(request.WebsitePassword))
            {
                var credential = website.WebsiteCredentials.FirstOrDefault(c => c.isActive);
                if (credential == null)
                {
                    credential = new WebsiteCredentialModel
                    {
                        WebsiteId = website.Id,
                        isActive = true
                    };
                    website.WebsiteCredentials.Add(credential);
                }

                if (!string.IsNullOrWhiteSpace(request.WebsiteUsername))
                {
                    credential.WebsiteUsernameFer = _crypto.GetEncryptedText(request.WebsiteUsername);
                    credential.WebsiteUsernameSha = _sha256.GetHash(request.WebsiteUsername);
                }

                if (!string.IsNullOrWhiteSpace(request.WebsitePassword))
                {
                    credential.WebsitePasswordFer = _crypto.GetEncryptedText(request.WebsitePassword);
                    credential.WebsitePasswordSha = passwordHash;
                    credential.WebsitePasswordLeakedCount = passwordLeakedCount;
                }
            }

            website.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new NewWebsiteResponse
            {
                Message = "Website credentials updated successfully."
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<NewWebsiteResponse> DeleteWebsiteCredentialAsync(Guid userUuid, Guid credentialUuid)
    {
        WebsiteCredentialModel? credential = await GetWebsiteCredentialByUuid(userUuid, credentialUuid);
        if (credential == null) throw new ArgumentException("Website credential do not found.");

        _context.WebsiteCredentials.Remove(credential);

        await _context.SaveChangesAsync();
        return new NewWebsiteResponse { Message = "Credentials deleted successfully" };
    }

}
