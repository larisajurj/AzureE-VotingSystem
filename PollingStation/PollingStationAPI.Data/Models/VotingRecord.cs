using Newtonsoft.Json;

namespace PollingStationAPI.Data.Models;

public class VotingRecord
{
    [JsonProperty("id")]
    public Guid Id { get; set; }
    public required string PollingStationId { get; set; }
    public required Guid VoterId { get; set; }
    public virtual RegisteredVoter? Voter { get; set; }
    public required string VotingStatus { get; set; }
    public string? AssociateCommitteeMemberId { get; set; }
    public string? Signature { get; set; }

}
