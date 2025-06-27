using PollingStationApp.Data.Helpers.Abstractions;

namespace PollingStationApp.Services;

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
public class SignalRService : IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly string _hubUrl;
    private string? _currentPollingStationId;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public event Action<string, string>? OnAppUnlocked;
    public event Func<VotingRecord, Task> OnVerifyVoterSignalReceived;
    public event Action? OnConnectionStateChanged;
    public event Func<Task> OnBoothStatusChanged;
    private readonly ITokenProvider _tokenProvider;
    public SignalRService(IConfiguration configuration, ITokenProvider tokenProvider) 
    {
        var apiURL = configuration["ConnectionStrings:PollingStationAPI"];
        _hubUrl = $"{apiURL}/voting";
        _tokenProvider = tokenProvider;
    }

    public async Task<bool?> InitializeAndRegisterAsync(string pollingStationId, ClaimsPrincipal user)
    {
        if (_hubConnection != null && _hubConnection.State != HubConnectionState.Disconnected)
        {
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                Console.WriteLine("SignalRService: Already initialized and connected for this circuit.");
                return true;
            }
            // If connected but for a different circuit, or in a connecting state, stop/dispose existing one.
            await StopAsync(); // Ensure clean state before reinitializing
            await DisposeCoreAsync(); // Dispose previous connection
        }

        _currentPollingStationId = pollingStationId;
        var tokenResult = await _tokenProvider.GetAccessTokenAsync(user);

            _hubConnection = new HubConnectionBuilder()
            .WithUrl(_hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(tokenResult); 
            })
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<VotingRecord>("ReceiveVerifiedVoterRecord", async (record) => 
        {
            Console.WriteLine($"SignalRService: ReceiveVerifiedVoterRecord message received: VoterId {record.VoterId}");
            if (OnVerifyVoterSignalReceived != null)
            {
                await OnVerifyVoterSignalReceived.Invoke(record);
                OnConnectionStateChanged?.Invoke();
            }
        });

        _hubConnection.On<string, string>("UnlockApp", (psId, cabin) => // psId for pollingStationId
        {
            Console.WriteLine($"SignalRService: UnlockApp received: PS {psId}, Cabin {cabin}");
            OnAppUnlocked?.Invoke(psId, cabin);
        });

        _hubConnection.On<string>("UpdateBoothStatus", async (pollingStationId) =>
        {
            // This code will execute when the server sends the message.
            Console.WriteLine($"Received UpdateBoothStatus for station: {pollingStationId}");

            // Optional: Check if the update is for the currently viewed station
            if (_currentPollingStationId != null && _currentPollingStationId == pollingStationId)
            {
                OnBoothStatusChanged?.Invoke();
            }
        });


        try
        {
            await _hubConnection.StartAsync();
            Console.WriteLine($"SignalRService: Connection started. ConnectionId: {_hubConnection.ConnectionId}");
            OnConnectionStateChanged?.Invoke();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalRService: Error starting connection or initial registration: {ex.Message}");
            OnConnectionStateChanged?.Invoke(); // Notify state changed even on error
            await DisposeCoreAsync(); // Clean up if start failed
            return false; // Or throw ex; to let caller handle
        }
    }



    public async Task RequestUnlockAppAsync(int cabinToUnlock) 
    {
        if (_hubConnection != null && IsConnected)
        {
            try
            {
                await _hubConnection.InvokeAsync("UnlockApp", _currentPollingStationId, cabinToUnlock.ToString());
                OnConnectionStateChanged?.Invoke();
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

    public async Task RequestDeleteSession(string pollingStationId, int cabinToDelete) 
    {
        if (_hubConnection != null && IsConnected)
        {
            try
            {
                await _hubConnection.InvokeAsync("DeleteSession", cabinToDelete.ToString(), _currentPollingStationId);
                OnConnectionStateChanged?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalRService: Error calling DeleteSession on server: {ex.Message}");
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