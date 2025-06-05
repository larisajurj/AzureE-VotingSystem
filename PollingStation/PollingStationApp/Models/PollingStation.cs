using Newtonsoft.Json;

namespace PollingStationApp.Models;

public class PollingStation
{
    [JsonProperty("id")]
    public required string Id { get; set; }
    public string? Name { get; set; }
    public string? ATU { get; set; }
    public string? Locality { get; set; }
    public string? Country { get; set; }
    public required int Siruta { get; set; }
    public required int MaxBooths { get; set; }
    public List<string> CommitteeMemberIds { get; set; } = new List<string>();
    public List<Booth> Booths { get; set; } = new List<Booth>();
}