namespace BankingOperationsService.Application.Contracts;

public interface ITransactionScopeService
{
    Task ExecuteInTransaction(Func<Task> operation, TimeSpan? timeout = null);

    Task<T> ExecuteInTransaction<T>(Func<Task<T>> operation, TimeSpan? timeout = null);

    Task ExecuteInTransactionWithRetries<T1>(Func<T1, Task> operation, TimeSpan? timeout = null)
        where T1 : notnull;

    Task ExecuteInTransactionWithRetries<T1, T2>(Func<T1, T2, Task> operation, TimeSpan? timeout = null)
        where T1 : notnull
        where T2 : notnull;

    Task ExecuteInTransactionWithRetries<T1, T2, T3>(Func<T1, T2, T3, Task> operation, TimeSpan? timeout = null)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull;
}
