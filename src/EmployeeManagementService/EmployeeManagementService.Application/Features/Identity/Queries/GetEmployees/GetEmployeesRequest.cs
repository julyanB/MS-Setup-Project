namespace EmployeeManagementService.Application.Features.Identity.Queries.GetEmployees;

public class GetEmployeesRequest
{
    public string? Role { get; set; }

    public string? Search { get; set; }

    public int Limit { get; set; } = 100;
}
