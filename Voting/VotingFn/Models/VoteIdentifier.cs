namespace VotingFn.Models;

public record VoteIdentifier (
	string  targetCandidateIdentifier,
    string? pollingStationIdFilter = null, 
    DateTime? dateFilter = null);

