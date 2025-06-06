using PollingStationAPI.Data.Models;
using PollingStationAPI.Data.Repository.Abstractions;
using PollingStationAPI.Service.Exceptions;
using PollingStationAPI.Service.Services.Abstractions;

namespace PollingStationAPI.Service.Services;

public class CommitteeMemberService : ICommitteeMemberService
{
    private readonly IRepository<CommitteeMember, string> _committeeMemberRepository;
    private readonly IRepository<PollingStation, string> _pollingStationRepository;

    public CommitteeMemberService(IRepository<CommitteeMember, string> committeeMemberRepository, IRepository<PollingStation, string> pollingStationRepository)
    {
        _committeeMemberRepository = committeeMemberRepository;
        _pollingStationRepository = pollingStationRepository;
    }

    public async Task AddCommitteMember(CommitteeMember committerMember)
    {
        await _committeeMemberRepository.Add(committerMember);
    }

    public async Task DeleteCommitteeMember(string committeeMemberId)
    {
        var committeeMember = await _committeeMemberRepository.GetById(committeeMemberId);
        if (committeeMember == null)
        {
            throw new NotFoundException($"Committee member with Id {committeeMemberId} not found");
        }

        //Delete the committee member from polling station
        if(committeeMember.PollingStationId != null){
            var assignedPollingStation = await _pollingStationRepository.GetById(committeeMember.PollingStationId);
            assignedPollingStation?.CommitteeMemberIds.Remove(committeeMemberId);
        }

        if (!(await _committeeMemberRepository.Delete(committeeMemberId)))
        {
            throw new Exception("Could not delete");
        }
    }

    public async Task<CommitteeMember?> GetCommitteeMemberByPollingStationIdAndRole(string pollingStationId, string role)
    {
        if (role == "President" || role == "Member")
        {
            var committeeMembers = await _committeeMemberRepository.Filter(m => m.PollingStationId == pollingStationId && m.Role == role);
            return committeeMembers.FirstOrDefault();
          
        }
        else
        {
            throw new Exception("Status must be President or Member");
        }
        


    }

    public async Task<CommitteeMember> GetCommitteMember(string committeeMemberId)
    {
        return await _committeeMemberRepository.GetById(committeeMemberId) ?? throw new NotFoundException($"Committee member with Id {committeeMemberId} not found");

    }

    public async Task<PollingStation?> GetPollingStationByCommitteeMemberId(string committeeMemberId)
    {
        var committeeMember = await _committeeMemberRepository.GetById(committeeMemberId);
        if (committeeMember == null)
        {
            throw new NotFoundException($"Committee member with Id {committeeMemberId} not found");
        }
        if (committeeMember.PollingStationId != null)
        {
            var assignedPollingStation = await _pollingStationRepository.GetById(committeeMember.PollingStationId);
            return assignedPollingStation;
        }
        else
        {
            throw new NotFoundException($"Committee member does not have a PollingStationId associated");

        }
    }

    public async Task UpdateCommitteeMember(CommitteeMember committerMember)
    {
        var existingCommitteeMember = await _committeeMemberRepository.GetById(committerMember.Id);
        if (existingCommitteeMember == null)
        {
            throw new NotFoundException($"Committee member with Id {committerMember.Id} not found");
        }
        existingCommitteeMember = committerMember;
        await _committeeMemberRepository.Update(existingCommitteeMember);

    }
}
