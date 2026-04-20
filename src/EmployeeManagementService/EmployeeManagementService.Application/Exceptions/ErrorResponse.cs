using EmployeeManagementService.Domain.Exceptions;

namespace EmployeeManagementService.Application.Exceptions;

public class ErrorResponse
{
    public ErrorResponse(ErrorCodes errorCode, string? errorMessage = null, IEnumerable<string>? errorMessages = null)
    {
        ErrorMessages = errorMessages?.ToList() ?? new List<string>()
        {
            errorMessage is not null ? errorMessage : "An error occurred."
        };
        ErrorCode = errorCode;
    }

    public ErrorResponse(ErrorCodes errorCode, IEnumerable<string> errorMessages)
    {
        ErrorMessages = [.. errorMessages];
        ErrorCode = errorCode;
    }

    public ErrorResponse()
    {
        ErrorMessages = new List<string>();
        ErrorCode = null;
    }

    public IList<string> ErrorMessages { get; set; }

    public ErrorCodes? ErrorCode { get; set; }

    public string GetErrorMessages() => string.Join(',', ErrorMessages);
}
