using Service.Models;

namespace Service.Services.Abstractions;

public interface IWebsiteService
{
	Task<RegistrationStatusDTO> checkRegistration(String cnp, String lastName, string apikey); 
}
