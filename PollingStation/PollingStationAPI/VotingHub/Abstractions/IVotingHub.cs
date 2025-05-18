namespace PollingStationAPI.VotingHub.Abstractions;

public interface IVotingHub 
{
    Task UnlockApp(string pollingStationId, string cabin);
    Task<int> RegisterSession(string sessionId, string pollingStationId);
    Task DeleteSession(int boothId, string sessionId, string pollingStationId);


}
