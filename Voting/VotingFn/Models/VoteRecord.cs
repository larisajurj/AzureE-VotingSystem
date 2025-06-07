namespace VotingFn.Models;

public class VoteRecord
{
	//public required Guid RecordId { get; set; }
	public required Guid? BallotId { get; set; }
	public required DateTime TimestampUtc { get; set; }
	public required string CandidateVoted { get; set; }
	public required PollingStationInfo PollingStation { get; set; }

	public VoteRecord() { }

	public VoteRecord(
		//Guid recordId,
		Guid? ballotId,
		DateTime timestampUtc,
		string candidateVoted,
		PollingStationInfo pollingStation)
	{
		//RecordId = recordId;
		BallotId = ballotId;
		TimestampUtc = timestampUtc;
		CandidateVoted = candidateVoted;
		PollingStation = pollingStation;
	}

	public string GetBlobPath(VoteRecord vote)
	{
		var date = vote.TimestampUtc.Date;

		string path = $"raw/" +
					  $"year={date:yyyy}/" +
					  $"month={date:MM}/" +
					  $"day={date:dd}/" +
					  $"station={vote.PollingStation?.Id}/" +
					  $"votes_{date:yyyyMMdd}.jsonl";

		return path;
	}
}
