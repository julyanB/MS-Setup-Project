namespace EmployeeManagementService.Application;

public class ConcurrencyConfiguration
{
    public int MaxRetries { get; set; } = 3;
}
