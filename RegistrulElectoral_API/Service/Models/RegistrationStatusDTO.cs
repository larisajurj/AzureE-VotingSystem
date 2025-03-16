using Service.Models.Enums;

namespace Service.Models;

public class RegistrationStatusDTO
{
	public required string CNP { get; set; }
	public required string LastName { get; set; }
	public required RegistrationStatusDetails Status { get; set; }
	public required string Details { get; set; }


}
