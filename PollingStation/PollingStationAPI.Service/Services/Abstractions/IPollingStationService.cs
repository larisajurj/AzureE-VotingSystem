using PollingStationAPI.Data.Models;

namespace PollingStationAPI.Service.Services.Abstractions;

public interface IPollingStationService
{
    public void AddTestData();

    //Returns Booth Number
    public Task<Booth> RegisterSession(string sessionId, string pollingStationId);
    public Task DeleteSession(int boothId, string pollingStationId);
    public Task<Booth> AddBooth(string pollingStationId);
    public Task<Booth> GetBooth(string pollingStationId, int boothId);
    public Task<PollingStation> GetPollingStation(string pollingStationId);
    public Task<Booth> UpdateStatusBooth(string pollingStationId, int boothId, string state);

}
