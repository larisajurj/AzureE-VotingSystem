namespace VotingApp.Models;

public class Ballot
{
    //public required Guid RecordId { get; set; }
    public required Guid? BallotId { get; set; }
    public required DateTime TimestampUtc { get; set; }
    public required Guid CandidateVoted { get; set; }
    public required PollingStationInfo PollingStation { get; set; }
}
