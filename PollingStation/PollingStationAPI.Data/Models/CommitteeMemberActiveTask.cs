using System.Text.Json.Serialization;

namespace PollingStationAPI.Data.Models;

public class CommitteeMemberActiveTask
{
    [JsonPropertyName("id")] 
    public required string CommitteeMemberId { get; set; }
    public Guid CurrentVoterId { get; set; }
    public required string PollingStationId { get; set; }
    public DateTime TaskStartTimeUtc { get; set; }
    public string? TaskType { get; set; }
}