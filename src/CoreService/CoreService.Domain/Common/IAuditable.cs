namespace CoreService.Domain.Common;

public interface IAuditable : ITrackable
{
    string? CreatedBy { get; set; }

    string? ModifiedBy { get; set; }
}
