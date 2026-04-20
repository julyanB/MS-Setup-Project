namespace EmployeeManagementService.Domain.Exceptions;

public abstract class BaseDomainException : Exception
{
    public new string Message { get; set; } = string.Empty;

    public ErrorCodes ErrorCode { get; set; } = ErrorCodes.DomainError;
}
