using PollingStationAPI.Data.Models;
using PollingStationAPI.Data.Repository.Abstractions;
using PollingStationAPI.Service.Exceptions;
using PollingStationAPI.Service.Services.Abstractions;

namespace PollingStationAPI.Service.Services;

public class VotingRecordService : IVotingRecordService
{
    private readonly IRepository<VotingRecord, Guid> _repository;
    private readonly IRepository<RegisteredVoter, Guid> _electoralRegisterRepository;
    private readonly IRepository<PollingStation, string> _pollingStationRepository;
    private readonly IRepository<CommitteeMember, string> _committeeMemberRepository;



    public VotingRecordService(IRepository<VotingRecord, Guid> repository, 
        IRepository<RegisteredVoter, Guid> electoralRegisterRepository, 
        IRepository<PollingStation, string> pollingStationRepository,
       IRepository<CommitteeMember, string> committeeMemberRepository)
    {
        _repository                  = repository;
        _electoralRegisterRepository = electoralRegisterRepository;
        _pollingStationRepository    = pollingStationRepository;
        _committeeMemberRepository   = committeeMemberRepository;
    }

    public async Task<VotingRecord> AddRecordAsync(VotingRecord record, string pollingStationId)
    {
        RegisteredVoter? associatedVoter;
        if (record.VoterId != Guid.Empty)
        {
            associatedVoter = await _electoralRegisterRepository.GetById(record.VoterId);
            if (associatedVoter == null)
                throw new NotFoundException($"Voter with ID {record.VoterId} is not present in the Electoral Register");
            record.Voter = associatedVoter;

            var records = await _repository.Filter(r => r.VoterId == record.VoterId);
            if (records != null && records.Count() > 0)
            {
                throw new AlreadyVotedException($"Voter with ID {record.VoterId} has already voted");
            }

            var committeeMembers = await _committeeMemberRepository.Filter(c => c.PollingStationId == pollingStationId);
            if (committeeMembers == null || committeeMembers.Count() == 0)
            {
                throw new NotFoundException($"Committee Members for polling station Id {pollingStationId} doe not exist");

            }
            if (associatedVoter.PollingStationId == pollingStationId)
            {
                record.AssociateCommitteeMemberId = committeeMembers.Where(c => c.Role == "Member").FirstOrDefault()?.Id;
            }
            else
            {
                record.AssociateCommitteeMemberId = committeeMembers.Where(c => c.Role == "President").FirstOrDefault()?.Id;
            }
        }

     
        await _repository.Add(record);

        return record;
    }

    public async Task DeleteVotingRecordAsync(Guid recordId)
    {
        VotingRecord? record = await _repository.GetById(recordId);
        if (record == null)
        {
            throw new NotFoundException($"Voter with '{recordId}' not found.");
        }
        await _repository.Delete(recordId);
    }

    public async Task<VotingRecord> GetVotingRecordAsync(Guid recordId)
    {
        VotingRecord? record = await _repository.GetById(recordId);
        if (record == null)
        {
            throw new NotFoundException($"Voter with '{recordId}' not found.");
        }

        if (record.VoterId != Guid.Empty)
        {
            var associatedVoter = await _electoralRegisterRepository.GetById(record.VoterId);
            record.Voter = associatedVoter;
        }

        return record;
    }

    public async Task<VotingRecord> GetVotingRecordByVoterIdAsync(Guid voterId)
    {
        var records = await _repository.Filter(r => r.VoterId == voterId);
        if (records == null || records.Count() == 0)
        {
            throw new NotFoundException($"Record associated with '{voterId}' not found.");
        }

        var record = records.FirstOrDefault();

        if (record == null)
        {
            throw new NotFoundException($"Record associated with '{voterId}' not found..");
        }

        if (record.VoterId != Guid.Empty)
        {
            var associatedVoter = await _electoralRegisterRepository.GetById(record.VoterId);
            record.Voter = associatedVoter;
        }
        
        return record;
    }

    public async Task<VotingRecord> UpdateRecordStatusAsync(Guid voterId, string status)
    {
        var records = await _repository.Filter(r => r.VoterId == voterId);
        if (records == null || records.Count() == 0)
        {
            throw new NotFoundException($"Record associated with '{voterId}' not found.");
        }
        var record = records.FirstOrDefault();

        if(record == null)
        {
            throw new NotFoundException($"Record associated with '{voterId}' not found..");
        }

        record.VotingStatus = status;
        await _repository.Update(record);
        return record ?? throw new NotFoundException($"Record associated with '{voterId}' not found..");
    }
}
