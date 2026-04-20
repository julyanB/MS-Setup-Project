
namespace EmployeeManagementService.Infrastructure.Configuration;

public class HangfireSettings
{
    public const string SectionName = "Hangfire";

    public string ConnectionString { get; set; } = string.Empty;

    public bool PrepareSchemaIfNecessary { get; set; } = true;

    public string SchemaName { get; set; } = string.Empty;

    public int WorkerCount { get; set; } = 0;

    public string[] Queues { get; set; } = ["default"];

    public TimeSpan JobExpirationTimeout { get; set; } = TimeSpan.FromDays(7);

    public TimeSpan QueuePollInterval { get; set; } = TimeSpan.FromSeconds(15);

    public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromMinutes(5);

    public TimeSpan SlidingInvisibilityTimeout { get; set; } = TimeSpan.FromMinutes(5);

    public bool DisableGlobalLocks { get; set; } = true;

    public HangfireServerToggle Server { get; set; } = new();

    public HangfireDashboardSettings Dashboard { get; set; } = new();
}

public class HangfireServerToggle
{
    public bool Enabled { get; set; } = true;
}

public class HangfireDashboardSettings
{
    public bool Enabled { get; set; } = true;

    public string Path { get; set; } = "/hangfire";

    public bool AllowAnonymous { get; set; } = false;

    public string AppName { get; set; } = "EmployeeManagementService";
}
