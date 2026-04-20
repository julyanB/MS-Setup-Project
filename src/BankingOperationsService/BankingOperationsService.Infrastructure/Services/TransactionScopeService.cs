using System.Transactions;
using BankingOperationsService.Application;
using BankingOperationsService.Application.Contracts;
using BankingOperationsService.Application.Exceptions;
using BankingOperationsService.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BankingOperationsService.Infrastructure.Services;

public class TransactionScopeService : ITransactionScopeService
{
    private readonly ILogger<TransactionScopeService> _logger;
    private readonly IOptions<ConcurrencyConfiguration> _concurrencyConfiguration;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public TransactionScopeService(
        ILogger<TransactionScopeService> logger,
        IOptions<ConcurrencyConfiguration> concurrencyConfiguration,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _concurrencyConfiguration = concurrencyConfiguration;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task ExecuteInTransaction(Func<Task> operation, TimeSpan? timeout = null)
    {
        await ExecuteInTransaction(
            async () =>
            {
                await operation();
                return false;
            },
            timeout);
    }

    public async Task<T> ExecuteInTransaction<T>(Func<Task<T>> operation, TimeSpan? timeout = null)
    {
        using var ts = new TransactionScope(
            TransactionScopeOption.Required,
            CreateOptions(timeout),
            TransactionScopeAsyncFlowOption.Enabled);

        try
        {
            var result = await operation();
            ts.Complete();
            return result;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to process the operation in transaction. Rolling back!");
            throw;
        }
    }

    public async Task ExecuteInTransactionWithRetries<T1>(Func<T1, Task> operation, TimeSpan? timeout = null)
        where T1 : notnull
    {
        await ExecuteInTransactionWithRetries(
            async () =>
            {
                await using var scope = _serviceScopeFactory.CreateAsyncScope();
                var service = scope.ServiceProvider.GetRequiredService<T1>();
                await operation(service);
                return false;
            },
            timeout);
    }

    public async Task ExecuteInTransactionWithRetries<T1, T2>(Func<T1, T2, Task> operation, TimeSpan? timeout = null)
        where T1 : notnull
        where T2 : notnull
    {
        await ExecuteInTransactionWithRetries(
            async () =>
            {
                await using var scope = _serviceScopeFactory.CreateAsyncScope();
                var service1 = scope.ServiceProvider.GetRequiredService<T1>();
                var service2 = scope.ServiceProvider.GetRequiredService<T2>();
                await operation(service1, service2);
                return false;
            },
            timeout);
    }

    public async Task ExecuteInTransactionWithRetries<T1, T2, T3>(Func<T1, T2, T3, Task> operation, TimeSpan? timeout = null)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
    {
        await ExecuteInTransactionWithRetries(
            async () =>
            {
                await using var scope = _serviceScopeFactory.CreateAsyncScope();
                var service1 = scope.ServiceProvider.GetRequiredService<T1>();
                var service2 = scope.ServiceProvider.GetRequiredService<T2>();
                var service3 = scope.ServiceProvider.GetRequiredService<T3>();
                await operation(service1, service2, service3);
                return false;
            },
            timeout);
    }

    private async Task<T> ExecuteInTransactionWithRetries<T>(Func<Task<T>> operation, TimeSpan? timeout = null)
    {
        var maxRetries = _concurrencyConfiguration.Value.MaxRetries;

        for (var retryAttempt = 1; ; retryAttempt++)
        {
            using var ts = new TransactionScope(
                TransactionScopeOption.Required,
                CreateOptions(timeout),
                TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                var result = await operation();
                ts.Complete();
                return result;
            }
            catch (DatabaseException exception)
            {
                _logger.LogWarning(exception, "Database exception hit, RetryAttempt: {RetryAttempt}", retryAttempt);

                if (retryAttempt > maxRetries)
                {
                    _logger.LogError(exception, "Database retry count reached! MaxRetries: {MaxRetries}", maxRetries);
                    throw;
                }
            }
            catch (BaseDomainException)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to process the operation in transaction. Rolling back!");
                throw;
            }
        }
    }

    private TransactionOptions CreateOptions(TimeSpan? timeout = null)
    {
        var options = new TransactionOptions
        {
            IsolationLevel = IsolationLevel.RepeatableRead
        };

        if (timeout.HasValue)
        {
            options.Timeout = timeout.Value;
        }

        return options;
    }
}
