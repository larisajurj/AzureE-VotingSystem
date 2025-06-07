using System.Collections.Concurrent;
using VotingApp.Services;
using VotingApp.Services.Abstractions;

namespace VotingApp.Factories;

public interface IUserOnlineServiceFactory
{
    IUserOnlineService GetOrCreate(string userOid);
    bool TryGet(string userOid, out IUserOnlineService? service);
}

public class UserOnlineServiceFactory : IUserOnlineServiceFactory
{
    private readonly ConcurrentDictionary<string, IUserOnlineService> _userServices = new();

    public IUserOnlineService GetOrCreate(string userOid)
    {
        return _userServices.GetOrAdd(userOid, _ => new UserOnlineService());
    }

    public bool TryGet(string userOid, out IUserOnlineService? service)
    {
        return _userServices.TryGetValue(userOid, out service);
    }
}
