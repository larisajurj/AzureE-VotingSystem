using Microsoft.AspNetCore.SignalR;
using PollingStationAPI.VotingHub.Abstractions;

namespace PollingStationAPI.VotingHub;

public class VotingHub : Hub<IVotingHub>
{
    public async Task UnlockApp(string userId, string cabin)
    {
        await Clients.All.UnlockApp(userId, cabin);
    }
}
