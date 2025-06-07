namespace Service.Models;

public class RegisteredVoter
{
    public Guid? Id { get; set; }
    public required string LastName { get; set; }
    public required string FirstName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public required string PlaceOfBirth { get; set; }
    public required string Gender { get; set; }
    public required string PollingStationId { get; set; }
}
