using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PollingStationAPI.Data.Models;
using PollingStationAPI.Service.Services.Abstractions;
using PollingStationAPI.VotingHub.Abstractions;

namespace PollingStationAPI.VotingHub;

[Authorize]
public class VotingHub : Hub<IVotingHub>
{
    private readonly IPollingStationService _pollingStationService;
    private readonly IVotingRecordService _recordService;
    public VotingHub(IPollingStationService pollingStationService, IVotingRecordService recordService)
    {
        _pollingStationService = pollingStationService;
        _recordService = recordService;
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

    public async Task DeleteSession(string boothId, string pollingStationId) 
    {
        Console.WriteLine($"Deleting session for PollingStationId: {pollingStationId}, BoothId: {boothId}");
        try
        {
            int cabinNr = Int32.Parse(boothId);
            await _pollingStationService.DeleteSession(cabinNr, pollingStationId);
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
        try
        {
            int cabinNr = Int32.Parse(cabin);

            await _pollingStationService.UpdateStatusBooth(pollingStationId, cabinNr, "unlocked");
            await Clients.All.UnlockApp(pollingStationId, cabin);
        }
        catch (HubException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error unlocking app {pollingStationId}:{cabin} : {ex.Message}");
            throw new HubException("Error unlocking app", ex); //Wrap the exception
        }
        
    }
    public async Task<VotingRecord?> VerifyVoter(Guid voterId, string pollingStationId)
    {
        Console.WriteLine($"VerifyVoter requested for voter with ID {voterId}, polling station: {pollingStationId}");
        try
        {
            var record = new VotingRecord() { 
                Id = Guid.NewGuid(),
                PollingStationId = pollingStationId, 
                VoterId = voterId, 
                VotingStatus = "Verified"
            };

            var createdRecord = await _recordService.AddRecordAsync(record, pollingStationId);

            if (createdRecord.AssociateCommitteeMemberId == null)
                throw new Exception("Could not associate member");

            await Clients.User(createdRecord.AssociateCommitteeMemberId)
                .ReceiveVerifiedVoterRecord(createdRecord);

            return createdRecord;
        }
        catch (HubException ex)
        {
            throw;
        }
        catch (Exception ex)
        {

            Console.WriteLine($"Error verifying app voter {voterId}, polling station {pollingStationId} : {ex.Message}");
            throw new HubException("Error verifying voter", ex); //Wrap the exception
        }
         
    }

}
