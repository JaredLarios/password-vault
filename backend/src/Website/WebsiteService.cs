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
}
