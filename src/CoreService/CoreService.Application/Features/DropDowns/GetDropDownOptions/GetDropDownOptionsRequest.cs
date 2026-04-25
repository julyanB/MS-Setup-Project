using Microsoft.AspNetCore.Mvc;

namespace CoreService.Application.Features.DropDowns.GetDropDownOptions;

public class GetDropDownOptionsRequest
{
    [FromRoute(Name = "flow")]
    public string Flow { get; set; } = null!;

    [FromRoute(Name = "key")]
    public string Key { get; set; } = null!;

    [FromQuery]
    public bool IncludeInactive { get; set; }
}
