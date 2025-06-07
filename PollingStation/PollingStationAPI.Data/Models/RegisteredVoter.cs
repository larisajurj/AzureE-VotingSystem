using Newtonsoft.Json;

namespace PollingStationAPI.Data.Models;

public class RegisteredVoter
{
    [JsonProperty("id")]
    public Guid Id { get; set; }
    public required string LastName { get; set; }
    public required string FirstName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public required string PlaceOfBirth { get; set; }
    public required string Gender { get; set; }
    /// <summary>
    /// The ID of the polling station where this voter is permanently assigned.
    /// </summary>
    public required string PollingStationId { get; set; }

}
