using PollingStationAPI.Data.Models;

namespace PollingStationAPI.Service.Services.Abstractions;

public interface ICandidateService
{
    Task<List<Candidate>> GetCandidates();
    Task<Candidate> AddCandidate(Candidate candidate);
    Task RemoveCandidate(Guid candidateId);
    Task<Candidate> GetCandidateById(Guid id);
    Task<Candidate> UpdateCandidate(Candidate candidate);
}
