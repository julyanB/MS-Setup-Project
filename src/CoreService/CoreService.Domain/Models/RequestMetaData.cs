using CoreService.Domain.Common;

namespace CoreService.Domain.Models;

public class RequestMetaData : Entity<int>
{
    public required string RequestType { get; set; }
    public required string Status { get; set; }

    public required string CreatedBy { get; set; }
    public required string ModifiedBy { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required DateTimeOffset UpdatedAt { get; set; }

    public bool Seen { get; set; }

    public string? AdditionalJsonData { get; set; }
}
