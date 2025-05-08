using System.Security.Claims;

namespace PollingStationApp.Data.Helpers.Abstractions;

public interface ITokenProvider
{
    Task<string> GetAccessTokenAsync(ClaimsPrincipal user);
}
