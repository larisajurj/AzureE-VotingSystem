using System.ComponentModel;

namespace Service.Models.Enums;

public enum RegistrationStatusDetails
{
	[Description("Votantul este inregistrat in registrul electoral")]
	SuccessfullValidation = 0,
	[Description("Votantul si-a exercitat deja dreptul de vot la aceste alegeri")]
	AlreadyVoted = 1,
	[Description("Votantul nu este inregistrat in registrul electoral")]
	NotValidated = 2
}
