using CoreService.Domain.Exceptions;

namespace CoreService.Application.Exceptions;

public class DatabaseException : Exception
{
    public DatabaseException(ErrorCodes code, string message, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }

    public ErrorCodes Code { get; }
}
