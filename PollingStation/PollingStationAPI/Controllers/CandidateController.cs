namespace PollingStationAPI.Controllers;


using Microsoft.AspNetCore.Mvc;
using PollingStationAPI.Data.Models;
using PollingStationAPI.Service.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")] 
public class CandidateController : ControllerBase
{
    private readonly ICandidateService _candidateService;

    public CandidateController(ICandidateService candidateService)
    {
        _candidateService = candidateService ?? throw new ArgumentNullException(nameof(candidateService));
    }

    /// <summary>
    /// Gets a list of all candidates.
    /// </summary>
    /// <returns>A list of candidates.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<Candidate>), 200)]
    public async Task<IActionResult> GetCandidates()
    {
        var candidates = await _candidateService.GetCandidates();
        return Ok(candidates);
    }

    /// <summary>
    /// Gets a specific candidate by their numeric ID.
    /// </summary>
    /// <param name="id">The numeric ID of the candidate (e.g., CandidateNumber).</param>
    /// <returns>The candidate if found; otherwise, 404 Not Found.</returns>
    [HttpGet("{id:int}", Name = "GetCandidateByNumericId")] // Route: /api/candidates/123
    [ProducesResponseType(typeof(Candidate), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCandidateByNumericId(Guid id)
    {
        var candidate = await _candidateService.GetCandidateById(id);
        if (candidate == null)
        {
            return NotFound($"Candidate with numeric ID {id} not found.");
        }
        return Ok(candidate);
    }

    /// <summary>
    /// Adds a new candidate.
    /// </summary>
    /// <param name="candidate">The candidate to add.</param>
    /// <returns>The created candidate with its assigned ID.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Candidate), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AddCandidate([FromBody] Candidate candidate)
    {
        if (candidate == null)
        {
            return BadRequest("Candidate data is null.");
        }

        try
        {
            var addedCandidate = await _candidateService.AddCandidate(candidate);
            
            return Ok(addedCandidate); 
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while adding the candidate.");
        }
    }

    /// <summary>
    /// Updates an existing candidate.
    /// </summary>
    /// <param name="id">The GUID ID (Cosmos DB ID) of the candidate to update.</param>
    /// <param name="candidateToUpdate">The updated candidate data.</param>
    /// <returns>The updated candidate if successful; otherwise, appropriate error status.</returns>
    [HttpPut("{id:guid}")] // Route: /api/candidates/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
    [ProducesResponseType(typeof(Candidate), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateCandidate(Guid id, [FromBody] Candidate candidateToUpdate)
    {
        if (candidateToUpdate == null)
        {
            return BadRequest("Candidate data is null.");
        }
        if (id != candidateToUpdate.Id)
        {
            return BadRequest("Route ID does not match candidate ID in the request body.");
        }

        try
        {
            var updatedCandidate = await _candidateService.UpdateCandidate(candidateToUpdate);
            if (updatedCandidate == null)
            {
                return NotFound($"Candidate with ID {id} not found for update.");
            }
            return Ok(updatedCandidate);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while updating the candidate.");
        }
    }

    /// <summary>
    /// Removes a candidate by their GUID ID (Cosmos DB ID).
    /// </summary>
    /// <param name="id">The GUID ID of the candidate to remove.</param>
    /// <returns>The removed candidate if successful; otherwise, 404 Not Found.</returns>
    [HttpDelete("{id:guid}")] // Route: /api/candidates/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
    [ProducesResponseType(typeof(Candidate), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RemoveCandidate(Guid id)
    {
                try
        {
            await _candidateService.RemoveCandidate(id);

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while removing the candidate.");
        }
    }
}