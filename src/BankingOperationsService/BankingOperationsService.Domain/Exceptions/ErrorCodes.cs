namespace BankingOperationsService.Domain.Exceptions;

public enum ErrorCodes
{
    ValidationError,
    ConcurrencyError,
    DuplicateKeyError,
    GenericError,
    NotFoundError,
    DomainError
}
