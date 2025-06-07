using Microsoft.Identity.Web;
using PollingStationApp.Data.Helpers.Abstractions;
using System.Security.Claims;

namespace PollingStationApp.Data.Helpers;

public class TokenProvider: ITokenProvider
{
private readonly ITokenAcquisition tokenAcquisition;
    private readonly IConfiguration configuration;
    private readonly MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler;

    public TokenProvider(IConfiguration configuration, ITokenAcquisition tokenAcquisition, 
        MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler)
    {
        this.configuration = configuration;
        this.tokenAcquisition = tokenAcquisition;
        this.consentHandler = consentHandler;
    }

    public async Task<string> GetAccessTokenAsync(ClaimsPrincipal user)
    {
        var scopes = configuration["AzureAd:Scopes"]?.Split(',') ?? Array.Empty<string>();

        if (user == null || !user.Identity!.IsAuthenticated)
        {
            throw new InvalidOperationException("User is not authenticated.");
        }
        var accountIdentifier = user.FindFirst("oid")?.Value;
        var tenantIdentifier = user.FindFirst("tid")?.Value;

        if (string.IsNullOrEmpty(accountIdentifier) || string.IsNullOrEmpty(tenantIdentifier))
        {
            throw new InvalidOperationException("User claims do not contain sufficient information to identify the account.");
        }

        string token = "";
        try
        {
            token = await GenerateNewTokenAsync(scopes);
            return token;
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            consentHandler.HandleException(ex);
            try
            {
                token = await GenerateNewTokenAsync(scopes);
                return token;
            }
            catch (Exception retryEx)
            {
                throw new InvalidOperationException("User interaction required to acquire token.", retryEx);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("User interaction required to acquire token.", ex);
        }
    }

    private async Task<string> GenerateNewTokenAsync(string[] scopes)
    {
        var token = await tokenAcquisition.GetAccessTokenForUserAsync(scopes);
        return token;
    }

}
