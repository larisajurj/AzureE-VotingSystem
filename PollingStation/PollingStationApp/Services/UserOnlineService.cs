using PollingStationApp.Models;
using PollingStationApp.Services.Abstractions;

namespace PollingStationApp.Services
{
    public class UserOnlineService : IUserOnlineService
    {
        public Guid InstanceId { get; } = Guid.NewGuid();
        public PollingStation? PollingStation { get; set; } 
        public CommitteeMember? CommitteeMember { get; set; }


        public event Action? OnChange;

        public TaskCompletionSource<bool> InitializationComplete { get; } = new();

        public UserOnlineService()
        {
        }

        private void NotifyStateChanged()
        {
            Console.WriteLine("NotifyStateChanged() triggered");
            OnChange?.Invoke();
        }
        public void MarkInitialized()
        {
            Console.WriteLine("UserOnlineService: Initialization marked complete.");

            InitializationComplete.TrySetResult(true);
        }


    }
}
