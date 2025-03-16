using Microsoft.AspNetCore.Mvc;
using Service.Models;
using Service.Models.Enums;
using Service.Services.Abstractions;

namespace RegistrulElectoralAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrulElectoralController : ControllerBase
{
	private IWebsiteService? websiteService { get; set; }

	private IConfiguration configuration;

	public RegistrulElectoralController(IWebsiteService websiteService, IConfiguration configuration)
	{
		this.websiteService = websiteService;
		this.configuration = configuration;
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
}
