using Microsoft.AspNetCore.Mvc;

namespace CoreService.Application.Features.DropDowns.SetDropDownOptions;

public class SetDropDownOptionsRequest
{
    [FromRoute(Name = "flow")]
    public string Flow { get; set; } = null!;

    [FromRoute(Name = "key")]
    public string Key { get; set; } = null!;

    [FromBody]
    public SetDropDownOptionsBody Body { get; set; } = new();
}

public class SetDropDownOptionsBody
{
    public IReadOnlyCollection<SetDropDownOptionItem> Options { get; set; } = [];
}

public class SetDropDownOptionItem
{
    public string Code { get; set; } = null!;

    public string Label { get; set; } = null!;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public string? MetadataJson { get; set; }
}
