using Microsoft.AspNetCore.SignalR;

namespace BankingOperationsService.Web.Hubs;

// Strongly-typed hub — inherits Hub<IExampleHubClient> so all client calls are compile-time safe.
// To push events from a service, inject IHubContext<ExampleHub, IExampleHubClient>.
// Mapped in Program.cs via: app.MapHub<ExampleHub>("/hubs/example")
public class ExampleHub : Hub<IExampleHubClient>
{
    public async Task NotifyExampleEvent(int entityId, string message)
        => await Clients.All.ExampleEventOccurred(entityId, message);
}
