using Microsoft.AspNetCore.SignalR;

namespace PollingStationAPI.VotingHub;

public class OidUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        var oid = connection.User?.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
        return oid;
    }
}