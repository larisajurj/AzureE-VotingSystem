namespace PollingStationAPI.Controllers;

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using PollingStationAPI.Service.Services.Abstractions;
using Microsoft.AspNetCore.Http.HttpResults;

[ApiController]
[Route("api/[controller]")]
public class PollingStationController : ControllerBase
{
    private readonly IPollingStationService _service;

    public PollingStationController(IPollingStationService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Add()
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
}
