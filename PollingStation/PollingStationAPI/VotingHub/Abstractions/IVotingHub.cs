namespace PollingStationAPI.VotingHub.Abstractions;

public interface IVotingHub 
{
    Task UnlockApp(string userId, string cabin);
    Task<int> RegisterSession(string userId, string sessionId);
    Task OnDisconnectedAsync(Exception exception);


}
