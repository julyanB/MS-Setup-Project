using EmployeeManagementService.Domain.Exceptions;

namespace EmployeeManagementService.Application.Exceptions;

public class DatabaseException : Exception
{
    public DatabaseException(ErrorCodes code, string message, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }

    public ErrorCodes Code { get; }
}
