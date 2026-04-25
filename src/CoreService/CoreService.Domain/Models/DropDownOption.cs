using CoreService.Domain.Common;

namespace CoreService.Domain.Models;

public sealed class DropDownOption : Entity<int>
{
    public string Flow { get; set; } = null!;

    public string Key { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string Label { get; set; } = null!;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public string? MetadataJson { get; set; }
}
