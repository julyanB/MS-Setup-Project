namespace EmployeeManagementService.Domain.Common;

public abstract class Trackable<TId> : Entity<TId>, ITrackable
    where TId : struct
{
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
