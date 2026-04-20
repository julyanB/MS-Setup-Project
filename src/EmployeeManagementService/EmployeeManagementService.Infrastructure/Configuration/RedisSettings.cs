namespace EmployeeManagementService.Infrastructure.Configuration;

/// <summary>
/// Strongly-typed settings for Redis. Bound from redis.json (section: "Redis").
/// </summary>
public class RedisSettings
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; set; } = "localhost:6379";

    public string InstanceName { get; set; } = string.Empty;

    public int DefaultDatabase { get; set; } = 0;

    public bool AbortOnConnectFail { get; set; } = false;

    public int ConnectTimeout { get; set; } = 5000;

    public int SyncTimeout { get; set; } = 5000;

    public int ConnectRetry { get; set; } = 3;

    public int KeepAlive { get; set; } = 60;

    public bool Ssl { get; set; } = false;

    public string? Password { get; set; }

    public string ChannelPrefix { get; set; } = "EmployeeManagementService";

    public RedisSignalRSettings SignalR { get; set; } = new();
}

public class RedisSignalRSettings
{
    public bool Enabled { get; set; } = true;

    public string ChannelPrefix { get; set; } = "EmployeeManagementServiceSignalR";
}
