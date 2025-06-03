namespace PollingStationAPI.Service.Services.Abstractions;

public interface IVirtualAssistantService
{
    Task<string> GetAnswer(string question, CancellationToken cancellationToken = default);
}
