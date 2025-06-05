using System.Security.Claims;
using PollingStationApp.Models;

namespace PollingStationApp.Services.Abstractions;

public interface IPollingStationClient
{
    void SetUser(ClaimsPrincipal user);
    Task<PollingStation?> GetStationById(string pollingStationId);
    Task<Booth?> GetBoothById(string pollingStationId, int boothId);
    Task<PollingStation?> GetStationByUserId();
    Task<CommitteeMember?> GetCommitteeMember();
    Task SaveSignature(Guid recordId, string signature);
    Task<List<VotingRecord>?> GetRecordsByStatus(string assignedMemberId, string status);
    Task<List<Booth>> GetBooths(string pollingStationId);
    Task DeleteBoothSession(string pollingStationId, int boothId);
    Task ChangeStatusOfRecord(Guid voterId, string status);
    Task<string> GetAnswer(string question);
    Task<List<Candidate>> GetCandidates();
    Task<List<VoteBallot>?> GetVotesForCandidateAsync(VoteIdentifier identifier);

}
