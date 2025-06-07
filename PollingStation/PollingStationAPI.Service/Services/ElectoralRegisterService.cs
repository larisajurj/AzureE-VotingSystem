using Microsoft.Azure.Cosmos.Serialization.HybridRow.RecordIO;
using PollingStationAPI.Data.Models;
using PollingStationAPI.Data.Repository.Abstractions;
using PollingStationAPI.Service.Exceptions;
using PollingStationAPI.Service.Services.Abstractions;

namespace PollingStationAPI.Service.Services;

public class ElectoralRegisterService : IElectoralRegisterService
{
    private readonly IRepository<RegisteredVoter, Guid> _repository;

    public ElectoralRegisterService(IRepository<RegisteredVoter, Guid> repository)
    {
        _repository = repository;
    }

    public async Task DeleteVoterAsync(Guid voterId)
    {
        RegisteredVoter? voter = await _repository.GetById(voterId);
        if (voter == null)
        {
            throw new NotFoundException($"Voter with '{voterId}' not found.");
        }
        await _repository.Delete(voterId);
    }

    public async Task<RegisteredVoter> GetVoterByIdAsync(Guid voterId)
    {
        RegisteredVoter? voter = await _repository.GetById(voterId);
        if (voter == null)
        {
            throw new NotFoundException($"Voter with '{voterId}' not found.");
        }
        return voter;
    }

    public async Task RegisterVoterAsync(RegisteredVoter voter)
    {
        RegisteredVoter? existingVoter = await _repository.GetById(voter.Id);
        if (existingVoter != null)
        {
            throw new Exception($"Voter with '{voter.Id}' already registered");
        }
        await _repository.Add(voter);
    }

    public async Task<RegisteredVoter> UpdateVoter(RegisteredVoter voter)
    {
        RegisteredVoter? existingVoter = await _repository.GetById(voter.Id);
        if (existingVoter == null)
        {
            throw new NotFoundException($"Voter with '{voter.Id}' not found.");
        }
        await _repository.Update(voter);
        return voter;
    }
 }
