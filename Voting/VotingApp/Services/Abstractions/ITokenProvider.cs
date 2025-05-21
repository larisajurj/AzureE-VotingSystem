using System.Security.Claims;

namespace VotingApp.Services.Abstractions;

public interface ITokenProvider
{
    Task<string> GetAccessTokenAsync(ClaimsPrincipal user);
}
