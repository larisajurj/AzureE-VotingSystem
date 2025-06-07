using PollingStationAPI.Data.Models;

namespace PollingStationAPI.Service.Services.Abstractions;

public interface IElectoralRegisterService
{
    Task RegisterVoterAsync(RegisteredVoter voter);
    Task DeleteVoterAsync(Guid voterId);
    Task<RegisteredVoter> GetVoterByIdAsync(Guid voterId);
    Task<RegisteredVoter> UpdateVoter(RegisteredVoter voter);

}
