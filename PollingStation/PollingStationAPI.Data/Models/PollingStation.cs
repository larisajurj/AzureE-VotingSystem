using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace PollingStationAPI.Data.Models;

public class PollingStation
{
    [JsonProperty("id")]
    public required string Id { get; set; }
    public string? Name { get; set; }
	public string? City { get; set; }
	public string? Country { get; set; }
    public required int Siruta {  get; set; }
    public required int MaxBooths { get; set; }
    public List<CommitteeMember> CommitteeMember { get; set; } = new List<CommitteeMember>();
	public List<Booth> Booths { get; set; } = new List<Booth>();
}
