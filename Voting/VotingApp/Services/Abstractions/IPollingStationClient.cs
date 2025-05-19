using System.Security.Claims;
using VotingApp.Models;

namespace VotingApp.Services.Abstractions;

public interface IPollingStationClient
{
    void SetUser(ClaimsPrincipal user);
    Task<PollingStation?> GetStationById(string pollingStationId);

}
