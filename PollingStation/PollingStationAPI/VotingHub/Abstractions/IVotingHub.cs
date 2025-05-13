namespace PollingStationAPI.VotingHub.Abstractions;

public interface IVotingHub 
{
    Task UnlockApp(string userId, string cabin);
}
