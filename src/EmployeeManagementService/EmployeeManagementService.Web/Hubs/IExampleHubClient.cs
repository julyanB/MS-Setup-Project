namespace EmployeeManagementService.Web.Hubs;

// Defines the methods the server can invoke on connected clients.
// Gives compile-time safety instead of string-based SendAsync("MethodName", ...) calls.
// Clients (JS, mobile, etc.) must implement these method names.
public interface IExampleHubClient
{
    Task ExampleEventOccurred(int entityId, string message);
}
