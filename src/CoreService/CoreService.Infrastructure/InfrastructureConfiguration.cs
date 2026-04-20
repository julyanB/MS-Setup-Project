using CoreService.Application;
using CoreService.Application.Contracts;
using CoreService.Application.ExternalClients;
using CoreService.Infrastructure.Configuration;
using CoreService.Infrastructure.Http;
using CoreService.Infrastructure.Persistence;
using CoreService.Infrastructure.Services;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Refit;
using StackExchange.Redis;

namespace CoreService.Infrastructure;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {

        services
            .AddConfigurations(configuration)
            .AddDatabase(configuration)
            .AddExternalApis(configuration)
            .AddHangfireJobs(configuration)
            .AddTransient<ITransactionScopeService, TransactionScopeService>();

        if (configuration.GetValue<bool>("Features:SignalR"))
        {
            services
                .AddRedis(configuration)
                .AddSignalRWithRedis(configuration, environment);
        }

        return services;
    }

    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<CoreServiceDbContext>(options => options
            .UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(CoreServiceDbContext)
                    .Assembly.FullName)));

        services.AddScoped<ICoreServiceDbContext>(provider =>
            provider.GetRequiredService<CoreServiceDbContext>());

        return services;
    }

    private static IServiceCollection AddRedis(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisSettings = configuration
            .GetSection(RedisSettings.SectionName)
            .Get<RedisSettings>() ?? new RedisSettings();

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options = ConfigurationOptions.Parse(redisSettings.ConnectionString);
            options.AbortOnConnectFail = redisSettings.AbortOnConnectFail;
            options.ConnectTimeout = redisSettings.ConnectTimeout;
            options.SyncTimeout = redisSettings.SyncTimeout;
            options.ConnectRetry = redisSettings.ConnectRetry;
            options.KeepAlive = redisSettings.KeepAlive;
            options.Ssl = redisSettings.Ssl;
            options.DefaultDatabase = redisSettings.DefaultDatabase;

            if (!string.IsNullOrWhiteSpace(redisSettings.Password))
            {
                options.Password = redisSettings.Password;
            }

            return ConnectionMultiplexer.Connect(options);
        });

        return services;
    }

    private static IServiceCollection AddSignalRWithRedis(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var redisSettings = configuration
            .GetSection(RedisSettings.SectionName)
            .Get<RedisSettings>() ?? new RedisSettings();

        var redisConnection = redisSettings.ConnectionString;
        var channelPrefix = !string.IsNullOrWhiteSpace(redisSettings.SignalR.ChannelPrefix)
            ? redisSettings.SignalR.ChannelPrefix
            : environment.ApplicationName.Replace(".", string.Empty);

        services
            .AddSignalR(options =>
            {
                options.KeepAliveInterval = TimeSpan.FromSeconds(10);
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(62);
            })
            .AddStackExchangeRedis(redisConnection, options =>
            {
                options.ConnectionFactory = async writer =>
                {
                    var redisOptions = ConfigurationOptions.Parse(redisConnection);
                    redisOptions.AbortOnConnectFail = redisSettings.AbortOnConnectFail;
                    redisOptions.Ssl = redisSettings.Ssl;
                    if (!string.IsNullOrWhiteSpace(redisSettings.Password))
                    {
                        redisOptions.Password = redisSettings.Password;
                    }

                    var connection = await ConnectionMultiplexer.ConnectAsync(redisOptions, writer);

                    connection.ConnectionFailed += (_, e) =>
                    {
                        var logger = writer as ILogger;
                        logger?.LogError(e.Exception, "SignalR Redis connection failed.");
                    };

                    if (!connection.IsConnected)
                    {
                        var logger = writer as ILogger;
                        logger?.LogError("SignalR did not connect to Redis.");
                    }

                    return connection;
                };

                options.Configuration.ChannelPrefix = RedisChannel.Pattern(channelPrefix);
            });

        return services;
    }

    // To add a new Refit integration:
    // 1. Create an interface in Application/Contracts/ with Refit attributes ([Get], [Post], etc.)
    // 2. Register it below with AddRefitClient<T>() pointing to its base URL from appsettings
    // 3. Add the base URL to appsettings.json under "ExternalApis"
    private static IServiceCollection AddExternalApis(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddTransient<HeaderPropagationHandler>();
        services.AddTransient<OutboundLoggingHandler>();

        services
            .AddRefitClient<IEmployeeManagementApi>()
            .ConfigureHttpClient(c =>
                c.BaseAddress = new Uri(configuration["ExternalApis:EmployeeManagement"]!))
            .AddHttpMessageHandler<HeaderPropagationHandler>()
            .AddHttpMessageHandler<OutboundLoggingHandler>();

        services
            .AddRefitClient<IBankingOperationsApi>()
            .ConfigureHttpClient(c =>
                c.BaseAddress = new Uri(configuration["ExternalApis:BankingOperations"]!))
            .AddHttpMessageHandler<HeaderPropagationHandler>()
            .AddHttpMessageHandler<OutboundLoggingHandler>();

        return services;
    }

    private static IServiceCollection AddConfigurations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .Configure<ConcurrencyConfiguration>(configuration.GetSection(nameof(ConcurrencyConfiguration)))
            .Configure<RedisSettings>(configuration.GetSection(RedisSettings.SectionName))
            .Configure<HangfireSettings>(configuration.GetSection(HangfireSettings.SectionName));

        return services;
    }

    private static IServiceCollection AddHangfireJobs(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var hangfireSettings = configuration
            .GetSection(HangfireSettings.SectionName)
            .Get<HangfireSettings>() ?? new HangfireSettings();

        var connectionString = !string.IsNullOrWhiteSpace(hangfireSettings.ConnectionString)
            ? hangfireSettings.ConnectionString
            : configuration.GetConnectionString("DefaultConnection");

        services.AddHangfire(config =>
        {
            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                {
                    PrepareSchemaIfNecessary = hangfireSettings.PrepareSchemaIfNecessary,
                    SchemaName = string.IsNullOrWhiteSpace(hangfireSettings.SchemaName)
                        ? "HangFire"
                        : hangfireSettings.SchemaName,
                    CommandBatchMaxTimeout = hangfireSettings.CommandTimeout,
                    SlidingInvisibilityTimeout = hangfireSettings.SlidingInvisibilityTimeout,
                    QueuePollInterval = hangfireSettings.QueuePollInterval,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = hangfireSettings.DisableGlobalLocks,
                });
        });

        if (hangfireSettings.Server.Enabled)
        {
            services.AddHangfireServer(options =>
            {
                if (hangfireSettings.WorkerCount > 0)
                {
                    options.WorkerCount = hangfireSettings.WorkerCount;
                }

                options.Queues = hangfireSettings.Queues.Length > 0
                    ? hangfireSettings.Queues
                    : ["default"];
            });
        }

        return services;
    }
}
