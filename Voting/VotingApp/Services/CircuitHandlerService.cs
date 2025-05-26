using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using VotingApp.Factories;
using VotingApp.Services;


namespace BlazorCircuitHandler.Services;

public class CircuitHandlerService : CircuitHandler
{
    public string? CircuitId { get; set; }
    public event Action<string>? OnCircuitOpened;
    IUserOnlineServiceFactory _userOnlineServiceFactory;
    private readonly AuthenticationStateProvider _authStateProvider;
    public string? CabinNumber { get; set; }

    private SignalRService SignalRService { get; } // Readonly after constructor

    public CircuitHandlerService(IUserOnlineServiceFactory userOnlineServiceFactory, AuthenticationStateProvider authStateProvider, SignalRService signalRService)
    {
        this._userOnlineServiceFactory = userOnlineServiceFactory;
        _authStateProvider = authStateProvider;

        SignalRService = signalRService;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        CircuitId = circuit.Id;
        OnCircuitOpened?.Invoke(circuit.Id);
        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    public override async Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var userOid = user.FindFirst("oid")?.Value;

        if (!string.IsNullOrEmpty(userOid) && _userOnlineServiceFactory.TryGet(userOid, out var userOnlineService))
        {
            
			if(userOnlineService.PollingStation != null)
				await SignalRService.DeleteMySessionAsync(circuit.Id, CabinNumber, userOnlineService.PollingStation.Id);

			//await userOnlineService.DisconnectAsync(circuit.Id);
            
        }
        await base.OnCircuitClosedAsync(circuit, cancellationToken);
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        return base.OnConnectionDownAsync(circuit, cancellationToken);
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        return base.OnConnectionUpAsync(circuit, cancellationToken);
    }
}