using Microsoft.AspNetCore.SignalR;
using PollingStationAPI.VotingHub.Abstractions;
using System.Collections.Concurrent;

namespace PollingStationAPI.VotingHub;

public class VotingHub : Hub<IVotingHub>
{
    private const int MaxCabinsPerUser = 3;

    private static ConcurrentDictionary<string, List<(string sessionId, int cabinNumber)>> UserSessions = new();

    public Task<int> RegisterSession(string userId, string sessionId)
    {
        Console.WriteLine("Registering session " + sessionId);
        var sessions = UserSessions.GetOrAdd(userId, _ => new List<(string, int)>());

        lock (sessions)
        {
            if (sessions.Any(s => s.sessionId == sessionId))
                return Task.FromResult(sessions.First(s => s.sessionId == sessionId).cabinNumber);

            if (sessions.Count >= MaxCabinsPerUser)
            {
                //throw new HubException("Maximum number of cabins reached.");
                this.OnDisconnectedAsync(new HubException("Maximum number of cabins reached."));
            }

            // Find the lowest available cabin number
            var assignedCabins = sessions.Select(s => s.cabinNumber).ToHashSet();
            int nextCabin = Enumerable.Range(1, MaxCabinsPerUser).First(n => !assignedCabins.Contains(n));

            sessions.Add((sessionId, nextCabin));
            return Task.FromResult(nextCabin);
        }
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        foreach (var kvp in UserSessions)
        {
            var sessions = kvp.Value;
            lock (sessions)
            {
                sessions.RemoveAll(s => s.sessionId == Context.ConnectionId);
            }
        }

        return base.OnDisconnectedAsync(exception);
    }

    public async Task UnlockApp(string userId, string cabin)
    {
        await Clients.All.UnlockApp(userId, cabin);
    }
}
