namespace EmployeeManagementService.Domain.Exceptions;

public enum ErrorCodes
{
    ValidationError,
    ConcurrencyError,
    DuplicateKeyError,
    GenericError,
    NotFoundError,
    IdentityError,
    DomainError
}
