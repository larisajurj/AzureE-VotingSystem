using Newtonsoft.Json;

namespace PollingStationAPI.Data.Models;

public class CommitteeMember
{
    [JsonProperty("id")]
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Role { get; set; }
    public string? Email {  get; set; }
    public string? PollingStationId { get; set; }
}


