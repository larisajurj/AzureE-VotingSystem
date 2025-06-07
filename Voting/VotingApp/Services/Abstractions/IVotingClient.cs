using VotingApp.Models;

namespace VotingApp.Services.Abstractions;

public interface IVotingClient
{
    Task<bool> SendVoteAsync(Ballot vote, CancellationToken cancellationToken = default);
}
