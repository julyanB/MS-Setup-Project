namespace CoreService.Domain.Common;

public interface ITrackable
{
    DateTimeOffset CreatedAt { get; set; }

    DateTimeOffset UpdatedAt { get; set; }
}
