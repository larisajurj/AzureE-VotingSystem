using Newtonsoft.Json;

namespace PollingStationApp.Models;


public class RegisteredVoter
{
    [JsonProperty("id")]
    public Guid Id { get; set; } 
    public required string LastName { get; set; }
    public required string FirstName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public required string PlaceOfBirth { get; set; }
    public required string Gender { get; set; }
    public required string PollingStationId { get; set; }
}
