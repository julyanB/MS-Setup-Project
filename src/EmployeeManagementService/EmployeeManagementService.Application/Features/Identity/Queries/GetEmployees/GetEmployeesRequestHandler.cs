using EmployeeManagementService.Application.Exceptions;
using EmployeeManagementService.Application.Features.Identity;
using FluentValidation;

namespace EmployeeManagementService.Application.Features.Identity.Queries.GetEmployees;

public class GetEmployeesRequestHandler
{
    private readonly IUserPermissions _userPermissions;
    private readonly IValidator<GetEmployeesRequest> _validator;

    public GetEmployeesRequestHandler(
        IUserPermissions userPermissions,
        IValidator<GetEmployeesRequest> validator)
    {
        _userPermissions = userPermissions;
        _validator = validator;
    }

    public async Task<IReadOnlyList<EmployeeLookupItem>> Handle(
        GetEmployeesRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ModelValidationException(validationResult.Errors);
        }

        return await _userPermissions.GetEmployees(
            new EmployeeLookupRequest(
                request.Role,
                request.Search,
                request.Limit),
            cancellationToken);
    }
}
