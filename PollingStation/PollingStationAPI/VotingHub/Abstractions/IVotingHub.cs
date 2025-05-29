using PollingStationAPI.Data.Models;

namespace PollingStationAPI.VotingHub.Abstractions;

public interface IVotingHub 
{
    Task UnlockApp(string pollingStationId, string cabin);
    Task<int> RegisterSession(string sessionId, string pollingStationId);
    Task DeleteSession(string boothId, string pollingStationId);
    Task<RegisteredVoter> VerifyVoter(RegisteredVoter voter, string pollingStationId);
    Task ReceiveVerifiedVoterRecord(VotingRecord record);
}
