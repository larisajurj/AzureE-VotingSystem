namespace PollingStationAPI.Service.DTOs;

public class VoteBallot
{
    public required Guid? BallotId { get; set; }
    public required DateTime TimestampUtc { get; set; }
    public required string CandidateVoted { get; set; }
    public required PollingStationInfo PollingStation { get; set; }
}
