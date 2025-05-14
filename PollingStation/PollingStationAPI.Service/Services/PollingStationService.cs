using PollingStationAPI.Data.Models;
using PollingStationAPI.Data.Repository;
using PollingStationAPI.Data.Repository.Abstractions;
using PollingStationAPI.Service.Services.Abstractions;

namespace PollingStationAPI.Service.Services;

public class PollingStationService : IPollingStationService
{
    private readonly IRepository<PollingStation,int> _repository;

    public PollingStationService(IRepository<PollingStation, int> repository)
    {
        _repository = repository;
    }

    public async void AddTestData()
    {
        var pollingStation = new PollingStation
        {
            Id = "1",
            Name = "Central High School Gym",
            CommitteeMember = new List<CommitteeMember>
    {
        new CommitteeMember
        {
            Id = Guid.NewGuid(),
            Name = "Alice Johnson",
            Role = "Chairperson",
            Email = "alice.johnson@example.com"
        },
        new CommitteeMember
        {
            Id = Guid.NewGuid(),
            Name = "Bob Smith",
            Role = "Secretary",
            Email = "bob.smith@example.com"
        }
    }
        };

        // Add to Cosmos DB using your repository
        await _repository.Add(pollingStation);
    }
}
