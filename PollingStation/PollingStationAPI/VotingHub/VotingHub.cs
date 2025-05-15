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
        Console.WriteLine($"Registering session for User: {userId}, SessionId: {sessionId}");
        var sessions = UserSessions.GetOrAdd(userId, _ => new List<(string, int)>());

        lock (sessions) 
        {
            var existingSession = sessions.FirstOrDefault(s => s.sessionId == sessionId);
            if (existingSession != default)
            {
                Console.WriteLine($"Session {sessionId} already registered for user {userId} with cabin {existingSession.cabinNumber}.");
                return Task.FromResult(existingSession.cabinNumber);
            }

            // Check if the user has reached the max number of concurrent sessions (cabins)
            if (sessions.Count >= MaxCabinsPerUser)
            {
                Console.WriteLine($"User {userId} reached maximum number of cabins ({MaxCabinsPerUser}). Denying session {sessionId}.");
                throw new HubException($"Maximum number of cabins ({MaxCabinsPerUser}) reached for this user.");
            }

            // Find the lowest available cabin number
            var assignedCabins = sessions.Select(s => s.cabinNumber).ToHashSet();
            int nextCabin = 1;
            while (assignedCabins.Contains(nextCabin) && nextCabin <= MaxCabinsPerUser)
            {
                nextCabin++;
            }

            if (nextCabin > MaxCabinsPerUser) 
            {
                 Console.WriteLine($"User {userId} - No available cabin numbers (this should not happen if MaxCabinsPerUser check is correct). Denying session {sessionId}.");
                 throw new HubException("Error assigning cabin: No available cabin numbers.");
            }

            sessions.Add((sessionId, nextCabin));
            Console.WriteLine($"Assigned Cabin {nextCabin} to User {userId} for Session {sessionId}. Total sessions for user: {sessions.Count}");
            return Task.FromResult(nextCabin);
        }
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        string connectionId = Context.ConnectionId;
        Console.WriteLine($"Client disconnected: {connectionId}. Exception: {exception?.Message}");

        string? userIdToRemoveFrom = null;
        foreach (var kvp in UserSessions)
        {
            lock (kvp.Value) // Lock the specific user's list
            {
                if (kvp.Value.Any(s => s.sessionId == connectionId))
                {
                    userIdToRemoveFrom = kvp.Key;
                    break;
                }
            }
        }

        if (userIdToRemoveFrom != null && UserSessions.TryGetValue(userIdToRemoveFrom, out var sessions))
        {
            lock (sessions) 
            {
                var removedCount = sessions.RemoveAll(s => s.sessionId == connectionId);
                if (removedCount > 0)
                {
                    Console.WriteLine($"Removed {removedCount} session(s) for connection {connectionId} under user {userIdToRemoveFrom}. Remaining for user: {sessions.Count}");
                }
                if (sessions.Count == 0)
                {
                    UserSessions.TryRemove(userIdToRemoveFrom, out _);
                    Console.WriteLine($"User {userIdToRemoveFrom} has no active sessions. Removed from UserSessions.");
                }
            }
        }
        else
        {
            Console.WriteLine($"Could not find user for disconnected session {connectionId} to clean up.");
        }

        return base.OnDisconnectedAsync(exception);
    }

    public async Task UnlockApp(string userId, string cabin)
    {
        Console.WriteLine($"UnlockApp requested by a client for User: {userId}, Cabin: {cabin}");
        await Clients.All.UnlockApp(userId, cabin);
    }
}
