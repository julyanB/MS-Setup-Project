namespace EmployeeManagementService.Application.Exceptions;

public class IdentityException : Exception
{
    public IdentityException(string message)
        : base(message)
    {
    }

    public IdentityException(IEnumerable<string> errors)
        : base(string.Join(Environment.NewLine, errors))
    {
        Errors = errors;
    }

    public IEnumerable<string> Errors { get; } = [];
}
