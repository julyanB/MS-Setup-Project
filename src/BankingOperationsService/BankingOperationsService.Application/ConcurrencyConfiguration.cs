namespace BankingOperationsService.Application;

public class ConcurrencyConfiguration
{
    public int MaxRetries { get; set; } = 3;
}
