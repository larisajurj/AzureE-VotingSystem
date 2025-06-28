using PollingStationAPI.Data.Models;
using PollingStationAPI.VotingHub.Models;

namespace PollingStationAPI.VotingHub.Abstractions;

public interface IVotingHub 
{
    Task UnlockApp(string pollingStationId, string cabin);
    Task<RegisteredBoothHubResult> RegisterSession(string sessionId, string pollingStationId);
    Task DeleteSession(string boothId, string pollingStationId);
    Task ReceiveDeleteSession(int cabin);
    Task<RegisteredVoter> VerifyVoter(RegisteredVoter voter, string pollingStationId);
    Task ReceiveVerifiedVoterRecord(VotingRecord record);
    Task UpdateBoothStatus(string pollingStationId);
}
