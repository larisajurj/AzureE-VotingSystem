namespace VotingApp.Services;

using Microsoft.AspNetCore.SignalR;

// In your Blazor client project (e.g., Services/VotingService.cs)
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

public class SignalRService : IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly string _hubUrl;

    public string? CurrentSessionId => _hubConnection?.ConnectionId;
    public int? CurrentCabinNumber { get; private set; }
    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public event Action<string, string>? OnAppUnlocked; 
    public event Action? OnConnectionStateChanged;     

    public SignalRService()
    {
        _hubUrl = "http://localhost:5062/voting";
    }


    public async Task InitializeAsync(string userId)
    {
        if (_hubConnection != null && _hubConnection.State != HubConnectionState.Disconnected)
        {
            Console.WriteLine("Already initialized or connecting.");
            return;
        }

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_hubUrl) 
            .WithAutomaticReconnect() 
            .Build();

        _hubConnection.On<string, string>("UnlockApp", (user, cabin) =>
        {
            Console.WriteLine($"UnlockApp received from server: User {user}, Cabin {cabin}");
            OnAppUnlocked?.Invoke(user, cabin);
        });

        _hubConnection.Closed += async (error) =>
        {
            Console.WriteLine($"Connection closed: {error?.Message}");
            CurrentCabinNumber = null; 
            OnConnectionStateChanged?.Invoke();
        };

        _hubConnection.Reconnecting += (error) =>
        {
            Console.WriteLine($"Connection reconnecting: {error?.Message}");
            OnConnectionStateChanged?.Invoke();
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += async (newConnectionId) =>
        {
            Console.WriteLine($"Connection reconnected with ID: {newConnectionId}");
            // IMPORTANT: Re-register the session on reconnect if to maintain same cabin logic
            if (_hubConnection != null && !string.IsNullOrEmpty(userId))
            {
                try
                {
                    CurrentCabinNumber = await _hubConnection.InvokeAsync<int>("RegisterSession", userId, _hubConnection.ConnectionId);
                    Console.WriteLine($"Session re-registered. User: {userId}, New SessionId: {_hubConnection.ConnectionId}, Cabin: {CurrentCabinNumber}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to re-register session: {ex.Message}");
                    // Handle failure, maybe stop connection or notify user
                    CurrentCabinNumber = null;
                    await StopAsync(); // Or some other error handling
                }
            }
            OnConnectionStateChanged?.Invoke();
        };


        await StartConnectionAsync(userId);
    }

    private async Task StartConnectionAsync(string userId)
    {
        if (_hubConnection == null || string.IsNullOrEmpty(userId))
        {
            Console.WriteLine("HubConnection not configured or userId missing.");
            return;
        }

        try
        {
            await _hubConnection.StartAsync();
            Console.WriteLine($"Connected to VotingHub with SessionId: {_hubConnection.ConnectionId}");
            OnConnectionStateChanged?.Invoke();

            // Once connected, register the session to get the cabin number
            CurrentCabinNumber = await _hubConnection.InvokeAsync<int>("RegisterSession", userId, _hubConnection.ConnectionId);
            Console.WriteLine($"Session registered. User: {userId}, SessionId: {_hubConnection.ConnectionId}, Cabin: {CurrentCabinNumber}");
        }
        catch (HubException hex) // Catch specific HubException from RegisterSession
        {
            Console.WriteLine($"HubException during connection/registration: {hex.Message}");
            CurrentCabinNumber = null;
            await StopAsync(); 
            throw; 
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting or registering session: {ex.Message}");
            CurrentCabinNumber = null;
            OnConnectionStateChanged?.Invoke();
        }
    }

    public async Task StopAsync()
    {
        if (_hubConnection != null && _hubConnection.State != HubConnectionState.Disconnected)
        {
            await _hubConnection.StopAsync();
        }
    }
	public async Task RequestUnlockAppAsync(string userId, string cabin)
    {
        if (_hubConnection != null && IsConnected)
        {
            try
            {
                await _hubConnection.InvokeAsync("UnlockApp", userId, cabin);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling UnlockApp on server: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("Not connected. Cannot send UnlockApp request.");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            _hubConnection.Closed -= OnConnectionClosedHandler; // Example if you had a named handler
            _hubConnection.Reconnecting -= OnReconnectingHandler;
            _hubConnection.Reconnected -= OnReconnectedHandler;
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
            CurrentCabinNumber = null;
            OnConnectionStateChanged?.Invoke();
        }
    }

    private Task OnConnectionClosedHandler(Exception? error) { /* ... */ return Task.CompletedTask; }
    private Task OnReconnectingHandler(Exception? error) { /* ... */ return Task.CompletedTask; }
    private Task OnReconnectedHandler(string? newConnectionId) { /* ... */ return Task.CompletedTask; }
}