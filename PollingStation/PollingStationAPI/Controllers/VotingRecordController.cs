namespace PollingStationAPI.Controllers;

using Microsoft.AspNetCore.Mvc;
using PollingStationAPI.Data.Models;
using PollingStationAPI.Service.Services.Abstractions;
using System;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")] 
public class VotingRecordController : ControllerBase
{
    private readonly IVotingRecordService _votingRecordService;

    public VotingRecordController(IVotingRecordService votingRecordService)
    {
        _votingRecordService = votingRecordService ?? throw new ArgumentNullException(nameof(votingRecordService));
    }

    /// <summary>
    /// Adds a new voting record.
    /// </summary>
    /// <param name="record">The voting record to add.</param>
    /// <returns>The created voting record.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(VotingRecord), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VotingRecord>> AddVotingRecord([FromQuery] string pollingStationId, [FromQuery] Guid voterId )
    {
        if (pollingStationId == null || voterId == Guid.Empty)
        {
            return BadRequest("Voting record data is required.");
        }

        try
        {
            var newRecord = new VotingRecord()
            {
                Id = Guid.NewGuid(),
                PollingStationId = pollingStationId,
                VoterId = voterId,
                VotingStatus = "Verified",
                Timestamp = DateTime.Now

            };
            Console.WriteLine($"Is Id empty? {newRecord.Id == Guid.Empty}"); // This will print "False"
            Console.WriteLine($"Generated Id: {newRecord.Id}");
            var createdRecord = await _votingRecordService.AddRecordAsync(newRecord, pollingStationId);
            // Assuming AddRecordAsync assigns an ID and returns the full record
            return CreatedAtAction(nameof(GetVotingRecordById), new { recordId = createdRecord.Id }, createdRecord);
        }
        catch (Exception ex) // Catch potential exceptions from the service
        {
            // Log the exception ex
            return BadRequest(new { message = "Failed to add voting record.", error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a specific voting record by its unique ID.
    /// (Assumed from IVotingRecordService.GetVotingRecordAsync)
    /// </summary>
    /// <param name="recordId">The unique ID of the voting record.</param>
    /// <returns>The voting record if found; otherwise, NotFound.</returns>
    [HttpGet("{recordId:guid}", Name = "GetVotingRecordById")]
    [ProducesResponseType(typeof(VotingRecord), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VotingRecord>> GetVotingRecordById(Guid recordId)
    {
        if (recordId == Guid.Empty)
        {
            return BadRequest("Voting record ID cannot be empty.");
        }

        // Using the renamed GetVotingRecordAsync method from the interface for clarity
        var record = await _votingRecordService.GetVotingRecordAsync(recordId);

        if (record == null)
        {
            return NotFound($"Voting record with ID {recordId} not found.");
        }

        return Ok(record);
    }

    /// <summary>
    /// Gets a voting record by the voter's ID.
    /// </summary>
    /// <param name="voterId">The ID of the voter.</param>
    /// <returns>The voting record if found; otherwise, NotFound.</returns>
    [HttpGet("byVoter/{voterId:guid}")]
    [ProducesResponseType(typeof(VotingRecord), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VotingRecord>> GetVotingRecordByVoterId(Guid voterId)
    {
        if (voterId == Guid.Empty)
        {
            return BadRequest("Voter ID cannot be empty.");
        }

        var record = await _votingRecordService.GetVotingRecordByVoterIdAsync(voterId);

        if (record == null)
        {
            return NotFound($"Voting record for Voter ID {voterId} not found.");
        }

        return Ok(record);
    }

    /// <summary>
    /// Updates the status of a voting record for a specific voter.
    /// </summary>
    /// <param name="voterId">The ID of the voter whose record status is to be updated.</param>
    /// <param name="statusUpdateRequest">Object containing the new status.</param>
    /// <returns>The updated voting record if successful; otherwise, an error status.</returns>
    [HttpPatch("byVoter/{voterId:guid}/status")] // Using HttpPatch for partial update of status
    [ProducesResponseType(typeof(VotingRecord), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VotingRecord>> UpdateVoterRecordStatus(Guid voterId, [FromBody] string status)
    {
        if (voterId == Guid.Empty)
        {
            return BadRequest("Voter ID cannot be empty.");
        }
        if (status == null || string.IsNullOrWhiteSpace(status))
        {
            return BadRequest("New status is required.");
        }

        try
        {
            var updatedRecord = await _votingRecordService.UpdateRecordStatusAsync(voterId, status);
            if (updatedRecord == null)
            {
                return NotFound($"Voting record for Voter ID {voterId} not found for status update.");
            }
            return Ok(updatedRecord);
        }
        catch (Exception ex) // Catch potential exceptions from the service
        {
            // Log the exception ex
            // Check if it's a "not found" type of exception specifically if your service throws them
            return BadRequest(new { message = "Failed to update voting record status.", error = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a voting record by its ID.
    /// </summary>
    /// <param name="recordId">The ID of the voting record to delete.</param>
    /// <returns>NoContent if successful; otherwise, an error status.</returns>
    [HttpDelete("{recordId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)] // If you want to indicate if not found
    public async Task<ActionResult> DeleteVotingRecord(Guid recordId)
    {
        if (recordId == Guid.Empty)
        {
            return BadRequest("Voting record ID cannot be empty.");
        }

        try
        {
            await _votingRecordService.DeleteVotingRecordAsync(recordId);
            return NoContent(); 
        }
        catch (KeyNotFoundException knfEx)
        {
            // Log knfEx
            return NotFound(new { message = knfEx.Message });
        }
        catch (Exception ex) 
        {
            // Log the exception ex
            return BadRequest(new { message = "Failed to delete voting record.", error = ex.Message });
        }
    }

    [HttpPost("{recordId:guid}/signature")]

    public async Task<ActionResult> SaveSignature(Guid recordId, [FromBody] string signature)
    {
        if (recordId == Guid.Empty)
        {
            return BadRequest("Record ID cannot be empty.");
        }

        try
        {
            await _votingRecordService.SaveSignature(recordId, signature);
            return NoContent();
        }
        catch (KeyNotFoundException knfEx)
        {
            return NotFound(new { message = knfEx.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Failed to save signature.", error = ex.Message });
        }
    }

    [HttpGet("byAssignedMemberId/{memberId}/status/{status}")]
    public async Task<ActionResult<List<VotingRecord>>> GetVotingRecordsByStatusAndMember(string memberId, string status)
    {
        if (memberId == null)
        {
            return BadRequest("Member ID cannot be empty.");
        }
        try
        {
            var records = await _votingRecordService.GetRecordsByStatus(memberId, status);

            if (records == null || records.Count() == 0)
            {
                return NotFound($"Voting record for Member ID {memberId} not found.");
            }

            return Ok(records);
        }
        catch (KeyNotFoundException knfEx)
        {
            return NotFound(new { message = knfEx.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Failed to get records.", error = ex.Message });
        }
    }

}

