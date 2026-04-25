namespace CoreService.Application.Features.DropDowns;

public class DropDownOptionDetails
{
    public int Id { get; set; }

    public string Flow { get; set; } = null!;

    public string Key { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string Label { get; set; } = null!;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public string? MetadataJson { get; set; }
}
