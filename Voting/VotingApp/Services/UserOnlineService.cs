namespace VotingApp.Services
{
    public class UserOnlineService : IUserOnlineService
    {

        public string? PollingStationId { get; private set; } // Use private setters if set only internally
        public string? CabinNumber { get; private set; }
        public string? CircuitId { get; private set; }
        private SignalRService SignalRService { get; } // Readonly after constructor

        public UserOnlineService(SignalRService signalRService)
        {
            SignalRService = signalRService;
        }

        public void Connect(string circuitId, string cabinNumber, string pollingStationId) // Corrected param order
        {
            PollingStationId = pollingStationId;
            CircuitId = circuitId;
            CabinNumber = cabinNumber;
            Console.WriteLine($"UserOnlineService: Connected circuit {CircuitId}, PS {PollingStationId}, Cabin {CabinNumber}");
        }

        // Renamed from DisConnect to DisconnectAsync and returns Task
        public async Task DisconnectAsync(string circuitId)
        {
            // Only disconnect if the properties were set (i.e., Connect was called for this circuit)
            if (this.CircuitId == circuitId && !string.IsNullOrEmpty(CabinNumber) && !string.IsNullOrEmpty(PollingStationId))
            {
                Console.WriteLine($"UserOnlineService: Disconnecting circuit {circuitId}, Cabin {CabinNumber}, PS {PollingStationId}");
                // Pass the actual cabin and polling station ID associated with this circuit
                await SignalRService.DeleteMySessionAsync(circuitId, CabinNumber, PollingStationId);
                PollingStationId = null;
                CabinNumber = null;
                CircuitId = null;
            }
            else
            {
                Console.WriteLine($"UserOnlineService: Attempted to disconnect circuit {circuitId}, but it was not the active one or not fully connected.");
            }
        }
    }
}
