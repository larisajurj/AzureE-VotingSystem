using PollingStationAPI.Data.Models;

namespace PollingStationAPI.Service.Services.Abstractions;

public interface ICommitteeMemberService
{
    public Task<PollingStation?> GetPollingStationByCommitteeMemberId(string committeeMemberId);

    public Task AddCommitteMember(CommitteeMember committerMember);

    public Task<CommitteeMember> GetCommitteMember(string committeeMemberId);

    public Task UpdateCommitteeMember(CommitteeMember committerMember);

    public Task DeleteCommitteeMember(string committeeMemberId);

    public Task<CommitteeMember?> GetCommitteeMemberByPollingStationIdAndRole(string pollingStationId, string role);


}
