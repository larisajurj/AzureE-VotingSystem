namespace PollingStationAPI.Controllers;

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using PollingStationAPI.Service.Services.Abstractions;
using Microsoft.AspNetCore.Http.HttpResults;
using PollingStationAPI.Service.Exceptions;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PollingStationController : ControllerBase
{
    private readonly IPollingStationService _service;

    public PollingStationController(IPollingStationService service)
    {
        _service = service;
    }

    [HttpPost]
    public IActionResult Add()
    {
        try
        {
            _service.AddTestData();
            return Ok();
        }
        catch (Exception ex) {
            return NotFound(ex.Message);

        }
    }

    [HttpPost("{pollingStationId}/booths/register-session/{sessionId}")]
    public async Task<IActionResult> RegisterSession(string sessionId, string pollingStationId) {
        try
        {
            var booth = await _service.RegisterSession(sessionId, pollingStationId);
            return Ok(booth);
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

    [HttpPost("{pollingStationId}/booth/{boothId}/delete-session")]
    public async Task<IActionResult> DeleteSession(int boothId, string pollingStationId)
    {
        Console.WriteLine("DeleteSession called");
        try
        {
            await _service.DeleteSession(boothId, pollingStationId);
            return Ok();
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

    [HttpPost("{pollingStationId}/booths")]
    public async Task<IActionResult> AddBooth(string pollingStationId)
    {
        try
        {
            var booth = await _service.AddBooth(pollingStationId);
            return Ok(booth);
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

    [HttpGet("{pollingStationId}/booth/{boothId}")]
    public async Task<IActionResult> GetBooth(string pollingStationId, int boothId)
    {
        try
        {
            var booth = await _service.GetBooth(pollingStationId, boothId);
            return Ok(booth);
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

    [HttpGet("{pollingStationId}")]
    public async Task<IActionResult> GetPollingStation(string pollingStationId)
    {
        try
        {
            var pollingStation = await _service.GetPollingStation(pollingStationId);
            return Ok(pollingStation);
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
