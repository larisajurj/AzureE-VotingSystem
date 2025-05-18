using Microsoft.AspNetCore.SignalR;
using PollingStationAPI.Service.Services.Abstractions;
using PollingStationAPI.VotingHub.Abstractions;

namespace PollingStationAPI.VotingHub;

public class VotingHub : Hub<IVotingHub>
{
    private readonly IPollingStationService _pollingStationService;
    public VotingHub(IPollingStationService pollingStationService)
    {
        _pollingStationService = pollingStationService;
    }
    public async Task<int> RegisterSession(string sessionId, string pollingStationId) //Removed userId
    {
        Console.WriteLine($"Registering session for SessionId: {sessionId}, PollingStationId: {pollingStationId}");

        try
        {
            var booth = await _pollingStationService.RegisterSession(sessionId, pollingStationId);
            return booth.Id; 
        }
        catch (HubException)
        {
            throw; 
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error registering session: {ex.Message}");
            throw new HubException("Failed to register session.", ex); //Wrap the exception
        }
    }

    public async Task DeleteSession(int boothId, string pollingStationId) 
    {
        Console.WriteLine($"Deleting session for PollingStationId: {pollingStationId}, BoothId: {boothId}");

        try
        {
            await _pollingStationService.DeleteSession( boothId, pollingStationId);
        }
        catch (HubException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error registering session: {ex.Message}");
            throw new HubException("Failed to register session.", ex); //Wrap the exception
        }
    }

    public async Task UnlockApp(string pollingStationId, string cabin)
    {
        Console.WriteLine($"UnlockApp requested by a client polling station: {pollingStationId}, Cabin: {cabin}");
        await Clients.All.UnlockApp(pollingStationId, cabin);
    }
}
