namespace VotingApp.Services;

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

public class SignalRService : IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly string _hubUrl;
    private string? _currentCircuitId; 
    private string? _currentPollingStationId;
    public string? _assignedCabin {get; set;}

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public event Action<string, string>? OnAppUnlocked;
    public event Action? OnConnectionStateChanged;

    public SignalRService(IConfiguration configuration) 
    {
        var apiURL = configuration["ClientConfigurations:PollingStationClient:BaseURL"];
        _hubUrl = $"{apiURL}/voting";
    }

    public async Task<string?> InitializeAndRegisterAsync(string pollingStationId, string circuitId)
    {
        if (_hubConnection != null && _hubConnection.State != HubConnectionState.Disconnected)
        {
            if (_hubConnection.State == HubConnectionState.Connected && _currentCircuitId == circuitId)
            {
                Console.WriteLine("SignalRService: Already initialized and connected for this circuit.");
                return _assignedCabin; // Return existing cabin if already registered for this
            }
            // If connected but for a different circuit, or in a connecting state, stop/dispose existing one.
            await StopAsync(); // Ensure clean state before reinitializing
            await DisposeCoreAsync(); // Dispose previous connection
        }

        _currentCircuitId = circuitId; // Store for use in registration and re-registration
        _currentPollingStationId = pollingStationId;
        _assignedCabin = null; // Reset cabin

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_hubUrl, options =>
            {
                // options.AccessTokenProvider = ... // If using auth
            })
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<string, string>("UnlockApp", (psId, cabin) => // psId for pollingStationId
        {
            Console.WriteLine($"SignalRService: UnlockApp received: PS {psId}, Cabin {cabin}");
            OnAppUnlocked?.Invoke(psId, cabin);
        });

        //_hubConnection.Closed += HandleConnectionClosed;
        //_hubConnection.Reconnecting += HandleReconnecting;
        //_hubConnection.Reconnected += HandleReconnected;

        try
        {
            await _hubConnection.StartAsync();
            Console.WriteLine($"SignalRService: Connection started. ConnectionId: {_hubConnection.ConnectionId}");
            _assignedCabin = await RegisterSessionWithHubAsync(); // Returns cabin or error string
            OnConnectionStateChanged?.Invoke();
            return _assignedCabin;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalRService: Error starting connection or initial registration: {ex.Message}");
            _assignedCabin = $"Error: {ex.Message}";
            OnConnectionStateChanged?.Invoke(); // Notify state changed even on error
            await DisposeCoreAsync(); // Clean up if start failed
            return _assignedCabin; // Or throw ex; to let caller handle
        }
    }

    // Renamed for clarity
    private async Task<string?> RegisterSessionWithHubAsync()
    {
        if (_hubConnection?.State != HubConnectionState.Connected || string.IsNullOrEmpty(_currentCircuitId) || string.IsNullOrEmpty(_currentPollingStationId))
        {
            Console.WriteLine("SignalRService: Cannot register session. Not connected or missing IDs.");
            return "Error: Not connected or IDs missing.";
        }

        try
        {
            // IMPORTANT: Ensure "RegisterSession" on the hub expects the CircuitId
            int boothNumber = await _hubConnection.InvokeAsync<int>("RegisterSession", _currentCircuitId, _currentPollingStationId);
            _assignedCabin = boothNumber.ToString();
            Console.WriteLine($"SignalRService: Session registered with Hub. CircuitId: {_currentCircuitId}, PS: {_currentPollingStationId}, Cabin: {_assignedCabin}");
            return _assignedCabin;
        }
        
        catch (HubException hex)
        {
            Console.WriteLine($"SignalRService: HubException calling RegisterSession: {hex.Message}");
            return $"Error: {hex.Message}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalRService: Error calling RegisterSession: {ex.Message}");
            return $"Error: {ex.Message}";
        }
    }

    // Corrected DeleteMySession - now async Task and calls a different hub method
    public async Task DeleteMySessionAsync(string circuitId, string cabinNumber, string pollingStationId)
    {
        if (_hubConnection?.State == HubConnectionState.Connected && _assignedCabin != null && pollingStationId != null && circuitId!= null)
        {
            try
            {
                Console.WriteLine($"SignalRService: Requesting to unregister session. CircuitId: {circuitId}, Cabin: {_assignedCabin}, PS: {pollingStationId}");
                await _hubConnection.InvokeAsync("DeleteSession", _assignedCabin, pollingStationId);

                if (circuitId == _currentCircuitId)
                {
                    _assignedCabin = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalRService: Error calling UnregisterSession on hub: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("SignalRService: Cannot unregister session, not connected.");
        }
    }

    private async Task HandleConnectionClosed(Exception? error)
    {
        Console.WriteLine($"SignalRService: Connection closed. Error: {error?.Message}");
        if(_currentCircuitId != null && _assignedCabin != null && _currentPollingStationId != null)
            await DeleteMySessionAsync(_currentCircuitId, _assignedCabin, _currentPollingStationId);
        _assignedCabin = null; // Clear cabin as connection is lost
        OnConnectionStateChanged?.Invoke();
        //return Task.CompletedTask;
    }

    private Task HandleReconnecting(Exception? error)
    {
        Console.WriteLine($"SignalRService: Connection reconnecting. Error: {error?.Message}");
        OnConnectionStateChanged?.Invoke();
        return Task.CompletedTask;
    }

    private async Task HandleReconnected(string? newConnectionId)
    {
        Console.WriteLine($"SignalRService: Connection reconnected with new ConnectionId: {newConnectionId}. Old CircuitId was: {_currentCircuitId}");
        // Re-register using the original _currentCircuitId and _currentPollingStationId
        if (_currentCircuitId != null && _assignedCabin != null && _currentPollingStationId != null)
            await DeleteMySessionAsync(_currentCircuitId, _assignedCabin, _currentPollingStationId);

        if (!string.IsNullOrEmpty(_currentCircuitId) && !string.IsNullOrEmpty(_currentPollingStationId))
        {
            _assignedCabin = await RegisterSessionWithHubAsync(); // This will use _currentCircuitId
            // If RegisterSessionWithHubAsync fails, _assignedCabin will contain error
        }
        else
        {
            Console.WriteLine("SignalRService: Cannot re-register session on reconnect, original IDs missing.");
            _assignedCabin = "Error: Reconnect failed to re-register.";
        }
        OnConnectionStateChanged?.Invoke();
    }


    public async Task RequestUnlockAppAsync(string pollingStationToUnlock, string cabinToUnlock) // Parameters renamed for clarity
    {
        if (_hubConnection != null && IsConnected)
        {
            try
            {
                // Make sure your Hub's "UnlockApp" method expects these parameters
                await _hubConnection.InvokeAsync("UnlockApp", pollingStationToUnlock, cabinToUnlock);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalRService: Error calling UnlockApp on server: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("SignalRService: Not connected. Cannot send UnlockApp request.");
        }
    }

    public async Task StopAsync()
    {
        if (_hubConnection != null && _hubConnection.State != HubConnectionState.Disconnected)
        {
            Console.WriteLine("SignalRService: Stopping hub connection.");
            await _hubConnection.StopAsync();
        }
    }

    // Extracted core disposal logic to be callable from InitializeAndRegisterAsync if needed
    private async ValueTask DisposeCoreAsync()
    {
        if (_hubConnection != null)
        {
            _hubConnection.Closed -= HandleConnectionClosed;
            _hubConnection.Reconnecting -= HandleReconnecting;
            _hubConnection.Reconnected -= HandleReconnected;

            if (_hubConnection.State != HubConnectionState.Disconnected)
            {
                try
                {
                    await _hubConnection.StopAsync(); // Ensure it's stopped before disposing
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SignalRService: Exception during StopAsync in DisposeCoreAsync: {ex.Message}");
                }
            }
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
            _assignedCabin = null; 
            _currentCircuitId = null;
            _currentPollingStationId = null;
            OnConnectionStateChanged?.Invoke(); 
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeCoreAsync();
        GC.SuppressFinalize(this);
    }
}