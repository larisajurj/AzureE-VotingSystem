namespace VotingApp.Services;

public interface IUserOnlineService
{
    string? PollingStationId { get; }
    string? CabinNumber { get; }
    string? CircuitId { get; }
    void Connect(string circuitId, string cabinNumber, string pollingStationId);
    Task DisconnectAsync(string circuitId);
}
