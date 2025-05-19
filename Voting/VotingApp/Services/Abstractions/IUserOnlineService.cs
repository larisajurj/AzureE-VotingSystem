namespace VotingApp.Services.Abstractions;

public interface IUserOnlineService
{
    string? PollingStationId { get; }
    string? CabinNumber { get; }
    string? CircuitId { get; }
    event Action? OnChange;

    void Connect(string circuitId, string cabinNumber, string pollingStationId);
    Task DisconnectAsync(string circuitId);
}
