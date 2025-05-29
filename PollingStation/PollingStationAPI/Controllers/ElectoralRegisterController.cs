namespace PollingStationAPI.Controllers;

using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using PollingStationAPI.Service.Services.Abstractions;
using PollingStationAPI.Data.Models;
using PollingStationAPI.Service.Exceptions;

[ApiController]
[Route("api/[controller]")] 
public class ElectoralRegisterController : ControllerBase
{
    private readonly IElectoralRegisterService _electoralRegisterService;

    public ElectoralRegisterController(IElectoralRegisterService electoralRegisterService)
    {
        _electoralRegisterService = electoralRegisterService ?? throw new ArgumentNullException(nameof(electoralRegisterService));
    }

    /// <summary>
    /// Registers a new voter.
    /// </summary>
    /// <param name="voter">The voter details to register.</param>
    /// <returns>A confirmation of creation.</returns>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RegisterVoter([FromBody] RegisteredVoter voter)
    {
        if (voter == null || voter.Id == Guid.Empty) // Basic validation
        {
            return BadRequest("Voter data is invalid or ID is missing.");
        }

        try
        {
            await _electoralRegisterService.RegisterVoterAsync(voter);
            // Return 201 Created with a location header pointing to the GetVoterById action
            return CreatedAtAction(nameof(GetVoterById), new { voterId = voter.Id }, voter);
        }
        catch (Exception ex) // Catch potential exceptions from the service (e.g., duplicate)
        {
            // Log the exception ex
            return BadRequest(new { message = "Failed to register voter.", error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a registered voter by their ID.
    /// </summary>
    /// <param name="voterId">The unique ID of the voter.</param>
    /// <returns>The registered voter if found; otherwise, NotFound.</returns>
    [HttpGet("{voterId:guid}", Name = "GetVoterById")] // Added route constraint for GUID
    [ProducesResponseType(typeof(RegisteredVoter), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RegisteredVoter>> GetVoterById(Guid voterId)
    {
        if (voterId == Guid.Empty)
        {
            return BadRequest("Voter ID cannot be empty.");
        }

        try
        {
            var voter = await _electoralRegisterService.GetVoterByIdAsync(voterId);
            return Ok(voter);

        }
        catch (NotFoundException nfe)
        {
            return NotFound(nfe.Message);

        }
        catch(Exception e)
        {
            return BadRequest(e.Message);
        }


    }

    /// <summary>
    /// Updates an existing voter's details.
    /// </summary>
    /// <param name="voterId">The ID of the voter to update.</param>
    /// <param name="voterToUpdate">The updated voter details.</param>
    /// <returns>The updated voter if successful; otherwise, an error status.</returns>
    [HttpPut("{voterId:guid}")] // Route constraint for GUID
    [ProducesResponseType(typeof(RegisteredVoter), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RegisteredVoter>> UpdateVoter(Guid voterId, [FromBody] RegisteredVoter voterToUpdate)
    {
        if (voterId == Guid.Empty || voterToUpdate == null || voterId != voterToUpdate.Id)
        {
            return BadRequest("Voter ID mismatch or invalid voter data.");
        }

        try
        {
            var updatedVoter = await _electoralRegisterService.UpdateVoter(voterToUpdate);
    
            return Ok(updatedVoter);
        }
        catch (NotFoundException nfe)
        {
            return NotFound(nfe.Message);

        }
        catch (Exception ex) // Catch potential exceptions from the service
        {
            // Log the exception ex
            // Check if it's a "not found" type of exception specifically if your service throws them
            return BadRequest(new { message = "Failed to update voter.", error = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a voter by their ID.
    /// </summary>
    /// <param name="voterId">The ID of the voter to delete.</param>
    /// <returns>NoContent if successful; otherwise, an error status.</returns>
    [HttpDelete("{voterId:guid}")] // Route constraint for GUID
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)] 
    public async Task<ActionResult> DeleteVoter(Guid voterId)
    {
        if (voterId == Guid.Empty)
        {
            return BadRequest("Voter ID cannot be empty.");
        }

        try
        {
            await _electoralRegisterService.DeleteVoterAsync(voterId);
            return NoContent(); 
        }
        catch (NotFoundException knfEx) 
        {
            return NotFound(knfEx.Message);
        }
        catch (Exception ex) 
        {
            return BadRequest(new { message = "Failed to delete voter.", error = ex.Message });
        }
    }
}