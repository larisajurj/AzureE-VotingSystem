using VotingApp.Models;

namespace VotingApp.Services.Abstractions;

public interface IUserOnlineService
{
    public Guid InstanceId { get; } 
    PollingStation? PollingStation { get; set; }
    CommitteeMember? CommitteeMember { get; set; }

    event Action? OnChange;

    TaskCompletionSource<bool> InitializationComplete { get; } 

    void MarkInitialized();
}
