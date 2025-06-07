using Microsoft.AspNetCore.Mvc;
using PollingStationAPI.Data.Models;
using PollingStationAPI.Service.Exceptions;
using PollingStationAPI.Service.Services.Abstractions;

namespace PollingStationAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class CommitteeMemberController : ControllerBase
{
    private readonly ICommitteeMemberService _committeeMemberService;

    public CommitteeMemberController(ICommitteeMemberService committeeMemberService)
    {
        _committeeMemberService = committeeMemberService;
    }

    [HttpGet("{committeeMemberId}")]
    public async Task<IActionResult> GetPollingStationByCommitteeMemberId(string committeeMemberId)
    {
        try
        {
            var committeeMember = await _committeeMemberService.GetCommitteMember(committeeMemberId);
            return Ok(committeeMember);
        }
        catch (NotFoundException ex) 
        {
            return NotFound(ex.Message);

        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);

        }
    }

    [HttpPost]
    public async Task<IActionResult> AddCommitteeMember(CommitteeMember committeeMember)
    {
        try
        {
            if (committeeMember.Id == null)
                committeeMember.Id = (new Guid()).ToString();
            await _committeeMemberService.AddCommitteMember(committeeMember);
            return Ok(committeeMember);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);

        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);

        }
    }

}
