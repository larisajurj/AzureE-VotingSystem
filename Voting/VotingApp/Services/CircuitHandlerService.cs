using Microsoft.AspNetCore.Components.Server.Circuits;
using VotingApp.Services;


namespace BlazorCircuitHandler.Services;

public class CircuitHandlerService : CircuitHandler
{
    public string? CircuitId { get; set; }
    public event Action<string>? OnCircuitOpened;
    IUserOnlineService _userOnlineService;
    public CircuitHandlerService(IUserOnlineService useronlineservice)
    {
        this._userOnlineService = useronlineservice;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        CircuitId = circuit.Id;
        OnCircuitOpened?.Invoke(circuit.Id);
        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    public override async Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        if (CircuitId == circuit.Id)
        {
            await _userOnlineService.DisconnectAsync(circuit.Id); // Call the async version
            CircuitId = null;
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