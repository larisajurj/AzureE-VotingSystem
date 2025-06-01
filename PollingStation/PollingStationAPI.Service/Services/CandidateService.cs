using PollingStationAPI.Data.Models;
using PollingStationAPI.Data.Repository.Abstractions;
using PollingStationAPI.Service.Services.Abstractions;

namespace PollingStationAPI.Service.Services;

public class CandidateService : ICandidateService
{
    private readonly IRepository<Candidate, Guid> _candidateRepository;

    public CandidateService(IRepository<Candidate, Guid> candidateRepository)
    {
        _candidateRepository = candidateRepository ?? throw new ArgumentNullException(nameof(candidateRepository));
    }

    public async Task<Candidate> AddCandidate(Candidate candidate)
    {
        if (candidate == null)
            throw new ArgumentNullException(nameof(candidate));

        // Assuming Id should be set if it's new, or handled by repository/Cosmos
        if (candidate.Id == Guid.Empty)
        {
            candidate.Id = Guid.NewGuid(); // Generate a new Guid if not provided
        }

        await _candidateRepository.Add(candidate);
        // The repository's Add method doesn't return the entity.
        // We return the input candidate, assuming it's successfully added or an exception is thrown.
        return candidate;
    }

    public async Task<List<Candidate>> GetCandidates()
    {
        var candidates = await _candidateRepository.Filter(c => true); // Get all candidates
        return candidates.ToList();
    }

    public async Task<Candidate> GetCandidateById(Guid id)
    {
        // This implementation assumes your Candidate model has an integer property
        // (e.g., 'CandidateNumber') that corresponds to this 'id'.
        // If 'id' is meant to be part of the Guid, the service interface or this logic needs changing.
        var candidates = await _candidateRepository.Filter(c => c.Id == id);
        var candidate = candidates.FirstOrDefault();
        // Returns null if not found, which matches Task<Candidate> (nullable reference type for Candidate)
        return candidate;
    }

    public async Task RemoveCandidate(Guid candidateId)
    {
        if (candidateId == Guid.Empty )
            throw new ArgumentNullException(nameof(candidateId));

        bool deleted = await _candidateRepository.Delete(candidateId);

    }

    public async Task<Candidate> UpdateCandidate(Candidate candidate)
    {
        if (candidate == null)
            throw new ArgumentNullException(nameof(candidate));
        if (candidate.Id == Guid.Empty)
            throw new ArgumentException("Candidate ID cannot be empty for an update.", nameof(candidate.Id));

        // The repository's Update method returns the updated entity or null if not found.
        return await _candidateRepository.Update(candidate);
    }
}