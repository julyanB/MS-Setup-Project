using BankingOperationsService.Application.Common;
using Microsoft.AspNetCore.Http;

namespace BankingOperationsService.Infrastructure.Http;

public class HeaderPropagationHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HeaderPropagationHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var incoming = _httpContextAccessor.HttpContext?.Request.Headers;
        if (incoming is not null)
        {
            foreach (var name in Headers.GetAll())
            {
                if (request.Headers.Contains(name))
                {
                    continue;
                }

                if (incoming.TryGetValue(name, out var values) && values.Count > 0)
                {
                    request.Headers.TryAddWithoutValidation(name, values.ToArray());
                }
            }
        }

        return base.SendAsync(request, cancellationToken);
    }
}
