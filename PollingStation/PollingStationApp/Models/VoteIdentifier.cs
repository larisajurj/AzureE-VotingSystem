namespace PollingStationApp.Models;

public record VoteIdentifier(
    string? targetCandidateIdentifier,
    string? pollingStationIdFilter = null,
    DateTime? dateFilter = null,
    string? Locality = null,
    string? ATU = null);