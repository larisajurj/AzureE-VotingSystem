using Microsoft.AspNetCore.Mvc;
using Service.Models;
using Service.Models.Enums;
using Service.Services;
using Service.Services.Abstractions;

namespace RegistrulElectoralAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrulElectoralController : ControllerBase
{
	private IWebsiteService? websiteService { get; set; }

	private IConfiguration configuration;
	private SignalRService signalRService;
	public RegistrulElectoralController(IWebsiteService websiteService, IConfiguration configuration, SignalRService signalRService)
	{
		this.websiteService = websiteService;
		this.configuration  = configuration;
		this.signalRService = signalRService;

    }

	[HttpGet("checkRegistration")]
	public async Task<ActionResult> CheckRegistration(String cnp, String lastName)
	{
		RegistrationStatusDTO registrationStatus = null ;
		try
		{
		  registrationStatus = await websiteService.checkRegistration(cnp, lastName, configuration.GetValue<string>("CapSolverAPIKey") ?? "");
		}
		catch (Exception ex) {
			return BadRequest(ex.Message);
		}

		if (registrationStatus?.Status == RegistrationStatusDetails.SuccessfullValidation)
		{
			return Ok(registrationStatus);
		}
		else
		{
			return BadRequest(registrationStatus);
		}
	}

    [HttpGet("verifyVoter")]
    public async Task<ActionResult> VerifyVoter(
        [FromQuery] Guid voterId,
        [FromQuery] string pollingStationId,
        [FromQuery] string token) 
    {
        if (string.IsNullOrEmpty(pollingStationId) || voterId == Guid.Empty)
        {
            return BadRequest("Essential voter id and polling station ID are required.");
        }

		try
		{

            await signalRService.InitializeSignalR(token);
            await signalRService.RequestValidateVoter(voterId, pollingStationId);
            return Ok(new { message = $"Verification request for voter {voterId} sent to clients for polling station {pollingStationId}." });
        }
        catch (Exception e)
		{
			return BadRequest(e.Message);
		}
    }

}
