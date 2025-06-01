using Newtonsoft.Json;

namespace PollingStationAPI.Data.Models;

public class Candidate
{
    [JsonProperty("id")]
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Party { get; set; }
    public required string Color { get; set; }
}
