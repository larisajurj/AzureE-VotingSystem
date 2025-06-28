namespace PollingStationAPI.VotingHub.Models;

public class RegisteredBoothHubResult
{
    public bool Success { get; set; }
    public ErrorType? ErrorType { get; set; }
    public string? ErrorMessage { get; set; }
    public int? AssignedBooth { get; set; }
}

public enum ErrorType
{
    Unknown = 0,
    MaxRegisteredBoothsExceeded = 1
}
