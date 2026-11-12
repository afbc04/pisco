using Microsoft.AspNetCore.Components.Server.Circuits;

namespace Pisco.Services;

public sealed class PresenceCircuit(AdminPresence presence) : CircuitHandler
{
    private bool admin;
    private bool connected = true;

    public void MarkAdmin()
    {
        if (admin)
            return;
        admin = true;
        if (connected)
            presence.Join();
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        if (!connected)
        {
            connected = true;
            if (admin)
                presence.Join();
        }
        return Task.CompletedTask;
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        if (connected)
        {
            connected = false;
            if (admin)
                presence.Leave();
        }
        return Task.CompletedTask;
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken) =>
        OnConnectionDownAsync(circuit, cancellationToken);
}
