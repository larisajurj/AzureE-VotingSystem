using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Service.Models;

namespace Service.Services;

public class SignalRService : IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly string _hubUrl;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public event Action? OnConnectionStateChanged;

    public SignalRService(IConfiguration configuration)
    {
        var apiURL = configuration["ConnectionStrings:PollingStationAPI"];
        _hubUrl = $"{apiURL}/voting";
    }

    public async Task InitializeSignalR()
    {
        if (_hubConnection != null && _hubConnection.State != HubConnectionState.Disconnected)
        {
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                Console.WriteLine("SignalRService: Already initialized and connected for this circuit.");
                return ;
            }
            // If connected but for a different circuit, or in a connecting state, stop/dispose existing one.
            await StopAsync(); // Ensure clean state before reinitializing
            await DisposeCoreAsync(); // Dispose previous connection
        }


        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_hubUrl, options =>
            {
                // options.AccessTokenProvider = ... // If using auth
            })
            .WithAutomaticReconnect()
            .Build();


        try
        {
            await _hubConnection.StartAsync();
            Console.WriteLine($"SignalRService: Connection started. ConnectionId: {_hubConnection.ConnectionId}");
            OnConnectionStateChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalRService: Error starting connection or initial registration: {ex.Message}");
            OnConnectionStateChanged?.Invoke(); 
            await DisposeCoreAsync(); 
        }
    }



    public async Task RequestValidateVoter(Guid voterId, string pollingStationId) // Parameters renamed for clarity
    {
        if (_hubConnection != null && IsConnected)
        {
            try
            {
                await _hubConnection.InvokeAsync("VerifyVoter", voterId, pollingStationId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalRService: Error calling VerifyVoter on server: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("SignalRService: Not connected. Cannot send VerifyVoter request.");
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

    private async ValueTask DisposeCoreAsync()
    {
        if (_hubConnection != null)
        {

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
            OnConnectionStateChanged?.Invoke();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeCoreAsync();
        GC.SuppressFinalize(this);
    }
}