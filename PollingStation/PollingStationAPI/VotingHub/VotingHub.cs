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
    private readonly ICommitteeMemberService _committeeMemberService;
    private readonly IVotingRecordService _recordService;
    private readonly ILogger<VotingHub> _logger;

    public VotingHub(IPollingStationService pollingStationService, IVotingRecordService recordService, ICommitteeMemberService committeeMemberService, ILogger<VotingHub> logger)
    {
        _pollingStationService = pollingStationService;
        _recordService = recordService;
        _committeeMemberService = committeeMemberService;
        _logger = logger;
    }

    public async Task<int> RegisterSession(string sessionId, string pollingStationId) //Removed userId
    {
        Console.WriteLine($"Registering session for SessionId: {sessionId}, PollingStationId: {pollingStationId}");
        _logger.LogInformation("Attempting to register session for PollingStationId: {PollingStationId}, SessionId: {SessionId}", pollingStationId, sessionId);

        try
        {
            var booth = await _pollingStationService.RegisterSession(sessionId, pollingStationId);
            _logger.LogInformation("Session registered successfully for PollingStationId: {PollingStationId}. Assigned BoothId: {BoothId}", pollingStationId, booth.Id);

            var president = await _committeeMemberService.GetCommitteeMemberByPollingStationIdAndRole(pollingStationId, "President");
            if (president == null){
                _logger.LogWarning("Could not find a President for PollingStationId: {PollingStationId}. Cannot send status update.", pollingStationId);
                throw new HubException("President not found for the polling station.");
            }
            _logger.LogInformation("Notifying President {PresidentId} to update status for PollingStationId: {PollingStationId}", president.Id, pollingStationId);
            await Clients.User(president.Id).UpdateBoothStatus(pollingStationId);

            return booth.Id; 
        }
        catch (HubException he)
        {
            _logger.LogError(he, "A handled HubException occurred during session registration for PollingStationId: {PollingStationId}.", pollingStationId);
            throw;

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error registering session: {ex.Message}");
            _logger.LogError(ex, "An unhandled exception occurred during session registration for PollingStationId: {PollingStationId}.", pollingStationId);
            throw new HubException("Failed to register session.", ex); //Wrap the exception
        }
    }

    public async Task DeleteSession(string boothId, string pollingStationId) 
    {
        Console.WriteLine($"Deleting session for PollingStationId: {pollingStationId}, BoothId: {boothId}");
        _logger.LogInformation("Attempting to delete session for BoothId: {BoothId} at PollingStationId: {PollingStationId}", boothId, pollingStationId);

        try
        {
            int cabinNr = Int32.Parse(boothId);

            var president = await _committeeMemberService.GetCommitteeMemberByPollingStationIdAndRole(pollingStationId, "President");
            if (president == null)
            {
                _logger.LogWarning("Could not find a President for PollingStationId: {PollingStationId} during session deletion.", pollingStationId);
                throw new HubException("President not found for the polling station.");
            }
            await _pollingStationService.UpdateStatusBooth(pollingStationId, cabinNr, "locked");
            _logger.LogInformation("Updated status for BoothId: {BoothId} to 'locked' at PollingStationId: {PollingStationId}", boothId, pollingStationId);

            await _pollingStationService.DeleteSession(cabinNr, pollingStationId);
            _logger.LogInformation("Deleted session data for BoothId: {BoothId} at PollingStationId: {PollingStationId}", boothId, pollingStationId);
            await Clients.User(president.Id).ReceiveDeleteSession(cabinNr);
            await Clients.User(president.Id).UpdateBoothStatus(pollingStationId);
            _logger.LogInformation("Notifying President {PresidentId} to update status for PollingStationId: {PollingStationId} after deletion.", president.Id, pollingStationId);

        }
        catch (HubException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error registering session: {ex.Message}");
            _logger.LogError(ex, "An unhandled exception occurred during session deletion for BoothId: {BoothId}, PollingStationId: {PollingStationId}", boothId, pollingStationId);
            throw new HubException("Failed to register session.", ex); //Wrap the exception
        }
    }

    public async Task UnlockApp(string pollingStationId, string cabin)
    {
        _logger.LogInformation("UnlockApp requested for PollingStationId: {PollingStationId}, Cabin: {Cabin}", pollingStationId, cabin);
        try
        {
            int cabinNr = Int32.Parse(cabin);
            var president = await _committeeMemberService.GetCommitteeMemberByPollingStationIdAndRole(pollingStationId, "President");
            if (president == null)
            {
                _logger.LogWarning("Could not find a President for PollingStationId: {PollingStationId} during UnlockApp request.", pollingStationId);
                throw new HubException("President not found for the polling station.");
            }

            await _pollingStationService.UpdateStatusBooth(pollingStationId, cabinNr, "unlocked");
            _logger.LogInformation("Updated status for Cabin: {Cabin} to 'unlocked' at PollingStationId: {PollingStationId}", cabin, pollingStationId);

            _logger.LogInformation("Sending UnlockApp and UpdateBoothStatus to President {PresidentId} for PollingStationId: {PollingStationId}", president.Id, pollingStationId);
            await Clients.User(president.Id).UnlockApp(pollingStationId, cabin);
            await Clients.User(president.Id).UpdateBoothStatus(pollingStationId);
        }
        catch (HubException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred during UnlockApp for PollingStationId: {PollingStationId}, Cabin: {Cabin}", pollingStationId, cabin);
            throw new HubException("Error unlocking app", ex); //Wrap the exception
        }
    }

    public async Task<VotingRecord?> VerifyVoter(Guid voterId, string pollingStationId)
    {
        _logger.LogInformation("VerifyVoter requested for VoterId: {VoterId} at PollingStationId: {PollingStationId}", voterId, pollingStationId);
        try
        {
            var record = new VotingRecord()
            {
                Id = Guid.NewGuid(),
                PollingStationId = pollingStationId,
                VoterId = voterId,
                VotingStatus = "Verified"
            };

            var createdRecord = await _recordService.AddRecordAsync(record, pollingStationId);
            _logger.LogInformation("Voter {VoterId} successfully verified with RecordId: {RecordId}", voterId, createdRecord.Id);

            if (string.IsNullOrEmpty(createdRecord.AssociateCommitteeMemberId))
            {
                _logger.LogError("Could not associate a committee member for verified VoterId {VoterId} at PollingStationId {PollingStationId}.", voterId, pollingStationId);
                throw new Exception("Could not associate committee member with the voting record.");
            }

            _logger.LogInformation("Sending verified voter record {RecordId} to CommitteeMemberId: {CommitteeMemberId}", createdRecord.Id, createdRecord.AssociateCommitteeMemberId);
            await Clients.User(createdRecord.AssociateCommitteeMemberId)
                .ReceiveVerifiedVoterRecord(createdRecord);

            return createdRecord;
        }
        catch (HubException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred during VerifyVoter for VoterId: {VoterId}, PollingStationId: {PollingStationId}", voterId, pollingStationId);
            throw new HubException("Error verifying voter", ex);
        }
    }

}
