using PollingStationAPI.Data.Models;

namespace PollingStationAPI.Service.Services.Abstractions;

public interface IVotingRecordService
{
    Task<VotingRecord> GetVotingRecordByVoterIdAsync(Guid voterId);
    Task<VotingRecord> GetVotingRecordAsync(Guid voterId);
    Task DeleteVotingRecordAsync(Guid recordId);
    Task<VotingRecord> AddRecordAsync(VotingRecord record, string pollingStationId);
    Task<VotingRecord> UpdateRecordStatusAsync(Guid voterId, string status);
    Task SaveSignature(Guid recordId, string signature);
    Task<List<VotingRecord>> GetRecordsByStatus(string assignedMemberId, string status);

}
