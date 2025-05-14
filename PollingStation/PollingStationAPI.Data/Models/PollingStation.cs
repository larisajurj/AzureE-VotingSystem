using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace PollingStationAPI.Data.Models;

public class PollingStation
{
    [JsonProperty("id")]
    public required string Id { get; set; }
    public string? Name { get; set; }
    public List<CommitteeMember> CommitteeMember { get; set; } = new List<CommitteeMember>();
}
